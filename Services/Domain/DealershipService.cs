using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Common;
using SunsetCars.DTOs.Requests;
using SunsetCars.DTOs.Responses;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Services.Domain;

public interface IDealershipService
{
    Task<Result<DealershipDto>> CreateDealershipAsync(CreateDealershipRequest request);
    Task<Result<DealershipDto>> UpdateDealershipAsync(UpdateDealershipRequest request);
    Task<Result<bool>> DeleteDealershipAsync(int id);
    Task<Result<DealershipDto>> GetDealershipByIdAsync(int id);
    Task<Result<List<DealershipDto>>> GetAllDealershipsAsync();
    Task<Result<List<DealershipDto>>> GetActiveDealershipsAsync();
}

public class DealershipService : IDealershipService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICepService _cepService;
    private readonly ILogger<DealershipService> _logger;

    public DealershipService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ICepService cepService,
        ILogger<DealershipService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _cepService = cepService;
        _logger = logger;
    }

    public async Task<Result<DealershipDto>> CreateDealershipAsync(CreateDealershipRequest request)
    {
        try
        {
            _logger.LogInformation("Criando concessionária: {Name}", request.Name);

            // Validar CEP
            var cepResult = await _cepService.GetAddressByCepAsync(request.ZipCode);
            if (cepResult == null || cepResult.Erro)
            {
                _logger.LogWarning("CEP inválido: {ZipCode}", request.ZipCode);
                return Result<DealershipDto>.Failure("CEP inválido. Não foi possível verificar o endereço.");
            }

            // Verificar se já existe concessionária com o mesmo nome
            var existingByName = await _unitOfWork.Dealerships
                .FindAsync(d => d.Name.ToLower() == request.Name.ToLower());

            if (existingByName.Any())
            {
                _logger.LogWarning("Tentativa de criar concessionária com nome duplicado: {Name}", request.Name);
                return Result<DealershipDto>.Failure("Já existe uma concessionária com este nome");
            }

            // Verificar se já existe concessionária com o mesmo e-mail
            var existingByEmail = await _unitOfWork.Dealerships
                .FindAsync(d => d.Email.ToLower() == request.Email.ToLower());

            if (existingByEmail.Any())
            {
                _logger.LogWarning("Tentativa de criar concessionária com e-mail duplicado: {Email}", request.Email);
                return Result<DealershipDto>.Failure("Já existe uma concessionária com este e-mail");
            }

            // Validar capacidade máxima
            if (request.MaxCapacity <= 0)
            {
                return Result<DealershipDto>.Failure("A capacidade máxima deve ser maior que zero");
            }

            // Criar nova concessionária
            var dealership = new Dealership
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                ZipCode = request.ZipCode.Replace("-", "").Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email.ToLower().Trim(),
                MaxCapacity = request.MaxCapacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Dealerships.AddAsync(dealership);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.DealershipsList);
            await _cacheService.RemoveAsync(CacheKeys.DealershipsActive);

            _logger.LogInformation("Concessionária criada com sucesso: {Id} - {Name}", dealership.Id, dealership.Name);

            var dto = MapToDto(dealership);
            return Result<DealershipDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar concessionária: {Name}", request.Name);
            return Result<DealershipDto>.Failure("Erro ao criar concessionária. Tente novamente.");
        }
    }

    public async Task<Result<DealershipDto>> UpdateDealershipAsync(UpdateDealershipRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando concessionária: {Id}", request.Id);

            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(request.Id);
            if (dealership == null)
            {
                _logger.LogWarning("Concessionária não encontrada: {Id}", request.Id);
                return Result<DealershipDto>.Failure("Concessionária não encontrada");
            }

            // Validar CEP
            var cepResult = await _cepService.GetAddressByCepAsync(request.ZipCode);
            if (cepResult == null || cepResult.Erro)
            {
                _logger.LogWarning("CEP inválido: {ZipCode}", request.ZipCode);
                return Result<DealershipDto>.Failure("CEP inválido. Não foi possível verificar o endereço.");
            }

            // Verificar se já existe outra concessionária com o mesmo nome
            var existingByName = await _unitOfWork.Dealerships
                .FindAsync(d => d.Name.ToLower() == request.Name.ToLower() && d.Id != request.Id);

            if (existingByName.Any())
            {
                _logger.LogWarning("Tentativa de atualizar para nome duplicado: {Name}", request.Name);
                return Result<DealershipDto>.Failure("Já existe outra concessionária com este nome");
            }

            // Verificar se já existe outra concessionária com o mesmo e-mail
            var existingByEmail = await _unitOfWork.Dealerships
                .FindAsync(d => d.Email.ToLower() == request.Email.ToLower() && d.Id != request.Id);

            if (existingByEmail.Any())
            {
                _logger.LogWarning("Tentativa de atualizar para e-mail duplicado: {Email}", request.Email);
                return Result<DealershipDto>.Failure("Já existe outra concessionária com este e-mail");
            }

            // Validar capacidade máxima
            if (request.MaxCapacity <= 0)
            {
                return Result<DealershipDto>.Failure("A capacidade máxima deve ser maior que zero");
            }

            // Atualizar dados
            dealership.Name = request.Name.Trim();
            dealership.Address = request.Address.Trim();
            dealership.City = request.City.Trim();
            dealership.State = request.State.Trim();
            dealership.ZipCode = request.ZipCode.Replace("-", "").Trim();
            dealership.Phone = request.Phone.Trim();
            dealership.Email = request.Email.ToLower().Trim();
            dealership.MaxCapacity = request.MaxCapacity;

            _unitOfWork.Dealerships.Update(dealership);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.DealershipsList);
            await _cacheService.RemoveAsync(CacheKeys.DealershipsActive);

            _logger.LogInformation("Concessionária atualizada com sucesso: {Id} - {Name}", dealership.Id, dealership.Name);

            var dto = MapToDto(dealership);
            return Result<DealershipDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar concessionária: {Id}", request.Id);
            return Result<DealershipDto>.Failure("Erro ao atualizar concessionária. Tente novamente.");
        }
    }

    public async Task<Result<bool>> DeleteDealershipAsync(int id)
    {
        try
        {
            _logger.LogInformation("Excluindo concessionária: {Id}", id);

            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(id);
            if (dealership == null)
            {
                _logger.LogWarning("Concessionária não encontrada para exclusão: {Id}", id);
                return Result<bool>.Failure("Concessionária não encontrada");
            }

            // Verificar se possui vendas associadas
            var hasSales = await _unitOfWork.Sales
                .FindAsync(s => s.DealershipId == id);

            if (hasSales.Any())
            {
                _logger.LogWarning("Tentativa de excluir concessionária com vendas associadas: {Id}", id);
                return Result<bool>.Failure("Não é possível excluir uma concessionária que possui vendas registradas");
            }

            _unitOfWork.Dealerships.Remove(dealership);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.DealershipsList);
            await _cacheService.RemoveAsync(CacheKeys.DealershipsActive);

            _logger.LogInformation("Concessionária excluída com sucesso: {Id}", id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir concessionária: {Id}", id);
            return Result<bool>.Failure("Erro ao excluir concessionária. Tente novamente.");
        }
    }

    public async Task<Result<DealershipDto>> GetDealershipByIdAsync(int id)
    {
        try
        {
            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(id);
            if (dealership == null)
            {
                return Result<DealershipDto>.Failure("Concessionária não encontrada");
            }

            var dto = MapToDto(dealership);
            return Result<DealershipDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar concessionária: {Id}", id);
            return Result<DealershipDto>.Failure("Erro ao buscar concessionária");
        }
    }

    public async Task<Result<List<DealershipDto>>> GetAllDealershipsAsync()
    {
        try
        {
            var dealerships = await _cacheService.GetOrCreateAsync(
                CacheKeys.DealershipsList,
                async () => await _unitOfWork.Dealerships.GetAllAsync(),
                TimeSpan.FromMinutes(30)
            );

            var dtos = dealerships.Select(MapToDto).ToList();
            return Result<List<DealershipDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as concessionárias");
            return Result<List<DealershipDto>>.Failure("Erro ao buscar concessionárias");
        }
    }

    public async Task<Result<List<DealershipDto>>> GetActiveDealershipsAsync()
    {
        try
        {
            var dealerships = await _cacheService.GetOrCreateAsync(
                CacheKeys.DealershipsActive,
                async () =>
                {
                    var all = await _unitOfWork.Dealerships.GetAllAsync();
                    return all.Where(d => d.IsActive).ToList();
                },
                TimeSpan.FromMinutes(30)
            );

            var dtos = dealerships.Select(MapToDto).ToList();
            return Result<List<DealershipDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar concessionárias ativas");
            return Result<List<DealershipDto>>.Failure("Erro ao buscar concessionárias ativas");
        }
    }

    private static DealershipDto MapToDto(Dealership dealership)
    {
        return new DealershipDto
        {
            Id = dealership.Id,
            Name = dealership.Name,
            Address = dealership.Address,
            City = dealership.City,
            State = dealership.State,
            ZipCode = dealership.ZipCode,
            Phone = dealership.Phone,
            Email = dealership.Email,
            MaxCapacity = dealership.MaxCapacity,
            IsActive = dealership.IsActive
        };
    }
}
