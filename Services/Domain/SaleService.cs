using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Common;
using SunsetCars.DTOs.Requests;
using SunsetCars.DTOs.Responses;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Services.Domain;

public interface ISaleService
{
    Task<Result<SaleDto>> CreateSaleAsync(CreateSaleRequest request, string userId);
    Task<Result<SaleDto>> UpdateSaleAsync(int id, UpdateSaleRequest request, string userId, string[] userRoles);
    Task<Result<List<SaleDto>>> GetSalesForUserAsync(string userId, string[] roles);
    Task<Result<SaleDetailsDto>> GetSaleByIdAsync(int id, string userId, string[] userRoles);
    Task<Result> DeleteSaleAsync(int id, string userId, string[] userRoles);
}

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProtocolGenerator _protocolGenerator;
    private readonly ILogger<SaleService> _logger;
    private readonly ICacheService _cacheService;

    public SaleService(
        IUnitOfWork unitOfWork,
        IProtocolGenerator protocolGenerator,
        ILogger<SaleService> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _protocolGenerator = protocolGenerator;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<SaleDto>> CreateSaleAsync(CreateSaleRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating sale for vehicle {VehicleId} by user {UserId}", request.VehicleId, userId);

            // Validações de negócio
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                return Result<SaleDto>.Failure("Veículo não encontrado");

            if (!vehicle.IsActive)
                return Result<SaleDto>.Failure("Veículo não está disponível para venda");

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<SaleDto>.Failure("Cliente não encontrado");

            if (!customer.IsActive)
                return Result<SaleDto>.Failure("Cliente não está ativo");

            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(request.DealershipId);
            if (dealership == null)
                return Result<SaleDto>.Failure("Concessionária não encontrada");

            if (!dealership.IsActive)
                return Result<SaleDto>.Failure("Concessionária não está ativa");

            // Validação de preço
            if (request.SalePrice > vehicle.Price)
                return Result<SaleDto>.Failure($"Preço de venda (R$ {request.SalePrice:N2}) não pode ser maior que o preço do veículo (R$ {vehicle.Price:N2})");

            if (request.SalePrice <= 0)
                return Result<SaleDto>.Failure("Preço de venda deve ser maior que zero");

            // Gerar protocolo único
            var protocol = await _protocolGenerator.GenerateAsync();

            // Criar venda
            var sale = new Sale
            {
                DealershipId = request.DealershipId,
                VehicleId = request.VehicleId,
                CustomerId = request.CustomerId,
                SalePrice = request.SalePrice,
                SalesPersonId = userId,
                Protocol = protocol,
                SaleDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Sales.AddAsync(sale);

            // Desativar o veículo após a venda
            vehicle.IsActive = false;
            _unitOfWork.Vehicles.Update(vehicle);

            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync(CacheKeys.SalesPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.ReportsPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.VehiclesPrefix);
            await _cacheService.RemoveAsync(CacheKeys.DashboardData);
            await _cacheService.RemoveAsync(CacheKeys.VehicleById(request.VehicleId));

            _logger.LogInformation("Sale created successfully with protocol {Protocol}", protocol);

            // Buscar venda com detalhes para retorno
            var createdSale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(sale.Id);
            var saleDto = MapToDto(createdSale!);

            return Result<SaleDto>.Success(saleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale for vehicle {VehicleId}", request.VehicleId);
            return Result<SaleDto>.Failure("Erro ao criar venda. Por favor, tente novamente.");
        }
    }

    public async Task<Result<SaleDto>> UpdateSaleAsync(int id, UpdateSaleRequest request, string userId, string[] userRoles)
    {
        try
        {
            _logger.LogInformation("Updating sale {SaleId} by user {UserId}", id, userId);

            var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            if (sale == null)
                return Result<SaleDto>.Failure("Venda não encontrada");

            // Verificar permissões
            if (!CanEditSale(sale, userId, userRoles))
                return Result<SaleDto>.Failure("Você não tem permissão para editar esta venda");

            // Validações de negócio
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                return Result<SaleDto>.Failure("Veículo não encontrado");

            if (request.SalePrice > vehicle.Price)
                return Result<SaleDto>.Failure($"Preço de venda (R$ {request.SalePrice:N2}) não pode ser maior que o preço do veículo (R$ {vehicle.Price:N2})");

            if (request.SaleDate > DateTime.Today)
                return Result<SaleDto>.Failure("Data da venda não pode ser futura");

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<SaleDto>.Failure("Cliente não encontrado");

            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(request.DealershipId);
            if (dealership == null)
                return Result<SaleDto>.Failure("Concessionária não encontrada");

            // Se o veículo foi trocado, gerenciar disponibilidade
            if (sale.VehicleId != request.VehicleId)
            {
                // Reativar veículo antigo
                var oldVehicle = await _unitOfWork.Vehicles.GetByIdAsync(sale.VehicleId);
                if (oldVehicle != null)
                {
                    oldVehicle.IsActive = true;
                    _unitOfWork.Vehicles.Update(oldVehicle);
                }

                // Desativar novo veículo
                if (!vehicle.IsActive)
                    return Result<SaleDto>.Failure("Veículo não está disponível para venda");

                vehicle.IsActive = false;
                _unitOfWork.Vehicles.Update(vehicle);
            }

            // Atualizar venda
            sale.DealershipId = request.DealershipId;
            sale.VehicleId = request.VehicleId;
            sale.CustomerId = request.CustomerId;
            sale.SalePrice = request.SalePrice;
            sale.SaleDate = request.SaleDate;

            _unitOfWork.Sales.Update(sale);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync(CacheKeys.SalesPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.VehiclesPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.ReportsPrefix);
            await _cacheService.RemoveAsync(CacheKeys.DashboardData);

            _logger.LogInformation("Sale {SaleId} updated successfully", id);

            var updatedSale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            var saleDto = MapToDto(updatedSale!);

            return Result<SaleDto>.Success(saleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sale {SaleId}", id);
            return Result<SaleDto>.Failure("Erro ao atualizar venda. Por favor, tente novamente.");
        }
    }

    public async Task<Result<List<SaleDto>>> GetSalesForUserAsync(string userId, string[] roles)
    {
        try
        {
            var sales = await _unitOfWork.Sales.GetSalesForUserAsync(userId, roles);
            var salesDto = sales.Select(MapToDto).ToList();

            return Result<List<SaleDto>>.Success(salesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales for user {UserId}", userId);
            return Result<List<SaleDto>>.Failure("Erro ao buscar vendas");
        }
    }

    public async Task<Result<SaleDetailsDto>> GetSaleByIdAsync(int id, string userId, string[] userRoles)
    {
        try
        {
            var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            if (sale == null)
                return Result<SaleDetailsDto>.Failure("Venda não encontrada");

            // Verificar permissões
            if (!CanViewSale(sale, userId, userRoles))
                return Result<SaleDetailsDto>.Failure("Você não tem permissão para visualizar esta venda");

            var saleDto = MapToDetailsDto(sale);
            return Result<SaleDetailsDto>.Success(saleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sale {SaleId}", id);
            return Result<SaleDetailsDto>.Failure("Erro ao buscar venda");
        }
    }

    public async Task<Result> DeleteSaleAsync(int id, string userId, string[] userRoles)
    {
        try
        {
            var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            if (sale == null)
                return Result.Failure("Venda não encontrada");

            // Verificar permissões (apenas Admin e Gerente podem excluir)
            if (!userRoles.Contains("Administrador") && !userRoles.Contains("Gerente"))
                return Result.Failure("Você não tem permissão para excluir vendas");

            // Reativar o veículo quando a venda for cancelada
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(sale.VehicleId);
            if (vehicle != null)
            {
                vehicle.IsActive = true;
                _unitOfWork.Vehicles.Update(vehicle);
            }

            sale.IsActive = false;
            _unitOfWork.Sales.Update(sale);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync(CacheKeys.SalesPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.VehiclesPrefix);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.ReportsPrefix);
            await _cacheService.RemoveAsync(CacheKeys.DashboardData);

            _logger.LogInformation("Sale {SaleId} deactivated and vehicle {VehicleId} reactivated by user {UserId}", id, sale.VehicleId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sale {SaleId}", id);
            return Result.Failure("Erro ao excluir venda");
        }
    }

    private bool CanViewSale(Sale sale, string userId, string[] roles)
    {
        if (roles.Contains("Administrador") || roles.Contains("Gerente"))
            return true;

        return roles.Contains("Vendedor") && sale.SalesPersonId == userId;
    }

    private bool CanEditSale(Sale sale, string userId, string[] roles)
    {
        if (roles.Contains("Administrador") || roles.Contains("Gerente"))
            return true;

        return roles.Contains("Vendedor") && sale.SalesPersonId == userId;
    }

    private SaleDto MapToDto(Sale sale) => new()
    {
        Id = sale.Id,
        Protocol = sale.Protocol,
        SaleDate = sale.SaleDate,
        SalePrice = sale.SalePrice,
        DealershipId = sale.DealershipId,
        DealershipName = sale.Dealership?.Name ?? "",
        VehicleId = sale.VehicleId,
        VehicleModel = sale.Vehicle?.Model ?? "",
        ManufacturerName = sale.Vehicle?.Manufacturer?.Name ?? "",
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.Name ?? "",
        CustomerCpf = sale.Customer?.CPF ?? "",
        SalesPersonName = sale.SalesPerson?.FullName ?? "",
        IsActive = sale.IsActive
    };

    private SaleDetailsDto MapToDetailsDto(Sale sale) => new()
    {
        Id = sale.Id,
        Protocol = sale.Protocol,
        SaleDate = sale.SaleDate,
        SalePrice = sale.SalePrice,
        DealershipId = sale.DealershipId,
        DealershipName = sale.Dealership?.Name ?? "",
        VehicleId = sale.VehicleId,
        VehicleModel = sale.Vehicle?.Model ?? "",
        ManufacturerName = sale.Vehicle?.Manufacturer?.Name ?? "",
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.Name ?? "",
        CustomerCpf = sale.Customer?.CPF ?? "",
        SalesPersonName = sale.SalesPerson?.FullName ?? "",
        IsActive = sale.IsActive,
        CreatedAt = sale.CreatedAt,
        Dealership = new DealershipDto
        {
            Id = sale.Dealership!.Id,
            Name = sale.Dealership.Name,
            Address = sale.Dealership.Address,
            City = sale.Dealership.City,
            State = sale.Dealership.State,
            ZipCode = sale.Dealership.ZipCode,
            Phone = sale.Dealership.Phone,
            Email = sale.Dealership.Email,
            MaxCapacity = sale.Dealership.MaxCapacity,
            IsActive = sale.Dealership.IsActive
        },
        Vehicle = new VehicleDto
        {
            Id = sale.Vehicle!.Id,
            Model = sale.Vehicle.Model,
            ManufacturerName = sale.Vehicle.Manufacturer?.Name ?? "",
            ManufacturerId = sale.Vehicle.ManufacturerId,
            Year = sale.Vehicle.Year,
            Price = sale.Vehicle.Price,
            IsActive = sale.Vehicle.IsActive
        },
        Customer = new CustomerDto
        {
            Id = sale.Customer!.Id,
            Name = sale.Customer.Name,
            Cpf = sale.Customer.CPF,
            Email = "", // Customer não tem Email
            Phone = sale.Customer.Phone,
            Address = "", // Customer não tem Address separado
            IsActive = sale.Customer.IsActive
        }
    };
}
