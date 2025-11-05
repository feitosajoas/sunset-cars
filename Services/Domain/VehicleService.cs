using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Common;
using SunsetCars.DTOs.Requests;
using SunsetCars.DTOs.Responses;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Services.Domain;

public interface IVehicleService
{
    Task<Result<VehicleDto>> CreateVehicleAsync(CreateVehicleRequest request, string userId);
    Task<Result<VehicleDto>> UpdateVehicleAsync(int id, UpdateVehicleRequest request, string userId);
    Task<Result<List<VehicleDto>>> GetAllVehiclesAsync();
    Task<Result<List<VehicleDto>>> GetActiveVehiclesAsync();
    Task<Result<VehicleDetailsDto>> GetVehicleByIdAsync(int id);
    Task<Result<List<VehicleDto>>> GetVehiclesByManufacturerAsync(int manufacturerId);
    Task<Result> DeleteVehicleAsync(int id, string userId);
}

public class VehicleService : IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehicleService> _logger;
    private readonly ICacheService _cacheService;

    public VehicleService(
        IUnitOfWork unitOfWork,
        ILogger<VehicleService> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<VehicleDto>> CreateVehicleAsync(CreateVehicleRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating vehicle {Model} by user {UserId}", request.Model, userId);

            // Validações de negócio
            var manufacturer = await _unitOfWork.Manufacturers.GetByIdAsync(request.ManufacturerId);
            if (manufacturer == null)
                return Result<VehicleDto>.Failure("Fabricante não encontrado");

            if (!manufacturer.IsActive)
                return Result<VehicleDto>.Failure("Fabricante não está ativo");

            // Validar ano
            var currentYear = DateTime.UtcNow.Year;
            if (request.Year > currentYear + 1)
                return Result<VehicleDto>.Failure($"Ano não pode ser maior que {currentYear + 1}");

            if (request.Year < 1900)
                return Result<VehicleDto>.Failure("Ano deve ser maior ou igual a 1900");

            // Validar preço
            if (request.Price <= 0)
                return Result<VehicleDto>.Failure("Preço deve ser maior que zero");

            // Criar veículo
            var vehicle = new Vehicle
            {
                ManufacturerId = request.ManufacturerId,
                Model = request.Model,
                Year = request.Year,
                Price = request.Price,
                Type = request.Type,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync("vehicles:");
            await _cacheService.RemoveAsync($"manufacturer:{request.ManufacturerId}");

            _logger.LogInformation("Vehicle created successfully: {VehicleId} - {Model}", vehicle.Id, vehicle.Model);

            // Buscar veículo com detalhes para retorno
            var createdVehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicle.Id);
            var vehicleDto = MapToDto(createdVehicle!);

            return Result<VehicleDto>.Success(vehicleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle {Model}", request.Model);
            return Result<VehicleDto>.Failure("Erro ao criar veículo. Por favor, tente novamente.");
        }
    }

    public async Task<Result<VehicleDto>> UpdateVehicleAsync(int id, UpdateVehicleRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Updating vehicle {VehicleId} by user {UserId}", id, userId);

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null)
                return Result<VehicleDto>.Failure("Veículo não encontrado");

            // Validações de negócio
            var manufacturer = await _unitOfWork.Manufacturers.GetByIdAsync(request.ManufacturerId);
            if (manufacturer == null)
                return Result<VehicleDto>.Failure("Fabricante não encontrado");

            if (!manufacturer.IsActive)
                return Result<VehicleDto>.Failure("Fabricante não está ativo");

            // Validar ano
            var currentYear = DateTime.UtcNow.Year;
            if (request.Year > currentYear + 1)
                return Result<VehicleDto>.Failure($"Ano não pode ser maior que {currentYear + 1}");

            if (request.Year < 1900)
                return Result<VehicleDto>.Failure("Ano deve ser maior ou igual a 1900");

            // Validar preço
            if (request.Price <= 0)
                return Result<VehicleDto>.Failure("Preço deve ser maior que zero");

            // Atualizar veículo
            vehicle.ManufacturerId = request.ManufacturerId;
            vehicle.Model = request.Model;
            vehicle.Year = request.Year;
            vehicle.Price = request.Price;
            vehicle.Type = request.Type;
            vehicle.Description = request.Description;

            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync("vehicles:");
            await _cacheService.RemoveAsync($"vehicle:{id}");
            await _cacheService.RemoveAsync($"manufacturer:{request.ManufacturerId}");

            _logger.LogInformation("Vehicle {VehicleId} updated successfully", id);

            var updatedVehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            var vehicleDto = MapToDto(updatedVehicle!);

            return Result<VehicleDto>.Success(vehicleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            return Result<VehicleDto>.Failure("Erro ao atualizar veículo. Por favor, tente novamente.");
        }
    }

    public async Task<Result<List<VehicleDto>>> GetAllVehiclesAsync()
    {
        try
        {
            var vehicles = await _cacheService.GetOrCreateAsync(
                CacheKeys.VehiclesList,
                async () => await _unitOfWork.Vehicles.GetAllAsync(),
                TimeSpan.FromMinutes(30));

            var vehiclesDto = vehicles.Select(MapToDto).ToList();
            return Result<List<VehicleDto>>.Success(vehiclesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all vehicles");
            return Result<List<VehicleDto>>.Failure("Erro ao buscar veículos");
        }
    }

    public async Task<Result<List<VehicleDto>>> GetActiveVehiclesAsync()
    {
        try
        {
            var vehicles = await _cacheService.GetOrCreateAsync(
                CacheKeys.VehiclesActive,
                async () => await _unitOfWork.Vehicles.GetActiveAsync(),
                TimeSpan.FromMinutes(30));

            var vehiclesDto = vehicles.Select(MapToDto).ToList();
            return Result<List<VehicleDto>>.Success(vehiclesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vehicles");
            return Result<List<VehicleDto>>.Failure("Erro ao buscar veículos ativos");
        }
    }

    public async Task<Result<VehicleDetailsDto>> GetVehicleByIdAsync(int id)
    {
        try
        {
            var vehicle = await _cacheService.GetOrCreateSingleAsync(
                CacheKeys.VehicleById(id),
                async () => await _unitOfWork.Vehicles.GetByIdAsync(id),
                TimeSpan.FromMinutes(30));

            if (vehicle == null)
                return Result<VehicleDetailsDto>.Failure("Veículo não encontrado");

            var vehicleDto = MapToDetailsDto(vehicle);
            return Result<VehicleDetailsDto>.Success(vehicleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle {VehicleId}", id);
            return Result<VehicleDetailsDto>.Failure("Erro ao buscar veículo");
        }
    }

    public async Task<Result<List<VehicleDto>>> GetVehiclesByManufacturerAsync(int manufacturerId)
    {
        try
        {
            var vehicles = await _cacheService.GetOrCreateAsync(
                CacheKeys.VehiclesByManufacturer(manufacturerId),
                async () => await _unitOfWork.Vehicles.GetByManufacturerIdAsync(manufacturerId),
                TimeSpan.FromMinutes(30));

            var vehiclesDto = vehicles.Select(MapToDto).ToList();
            return Result<List<VehicleDto>>.Success(vehiclesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles by manufacturer {ManufacturerId}", manufacturerId);
            return Result<List<VehicleDto>>.Failure("Erro ao buscar veículos do fabricante");
        }
    }

    public async Task<Result> DeleteVehicleAsync(int id, string userId)
    {
        try
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null)
                return Result.Failure("Veículo não encontrado");

            // Exclusão lógica
            vehicle.IsActive = false;
            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveByPrefixAsync("vehicles:");
            await _cacheService.RemoveAsync($"vehicle:{id}");
            await _cacheService.RemoveAsync($"manufacturer:{vehicle.ManufacturerId}");

            _logger.LogInformation("Vehicle {VehicleId} deactivated by user {UserId}", id, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return Result.Failure("Erro ao excluir veículo");
        }
    }

    private VehicleDto MapToDto(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        Model = vehicle.Model,
        ManufacturerName = vehicle.Manufacturer?.Name ?? "",
        ManufacturerId = vehicle.ManufacturerId,
        Year = vehicle.Year,
        Price = vehicle.Price,
        Type = (int)vehicle.Type,
        Description = vehicle.Description,
        IsActive = vehicle.IsActive,
        CreatedAt = vehicle.CreatedAt
    };

    private VehicleDetailsDto MapToDetailsDto(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        Model = vehicle.Model,
        ManufacturerName = vehicle.Manufacturer?.Name ?? "",
        ManufacturerId = vehicle.ManufacturerId,
        Year = vehicle.Year,
        Price = vehicle.Price,
        Type = (int)vehicle.Type,
        Description = vehicle.Description,
        IsActive = vehicle.IsActive,
        CreatedAt = vehicle.CreatedAt,
        Manufacturer = new ManufacturerDto
        {
            Id = vehicle.Manufacturer!.Id,
            Name = vehicle.Manufacturer.Name,
            Country = vehicle.Manufacturer.Country,
            FoundedYear = vehicle.Manufacturer.FoundedYear,
            Website = vehicle.Manufacturer.Website ?? string.Empty,
            IsActive = vehicle.Manufacturer.IsActive
        }
    };
}
