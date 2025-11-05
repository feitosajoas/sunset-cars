using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Common;
using SunsetCars.DTOs.Requests;
using SunsetCars.DTOs.Responses;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SunsetCars.Services.Domain;

public interface IManufacturerService
{
    Task<Result<ManufacturerDto>> CreateManufacturerAsync(CreateManufacturerRequest request);
    Task<Result<ManufacturerDto>> UpdateManufacturerAsync(UpdateManufacturerRequest request);
    Task<Result<bool>> DeleteManufacturerAsync(int id);
    Task<Result<ManufacturerDto>> GetManufacturerByIdAsync(int id);
    Task<Result<List<ManufacturerDto>>> GetAllManufacturersAsync();
    Task<Result<List<ManufacturerDto>>> GetActiveManufacturersAsync();
}

public class ManufacturerService : IManufacturerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ManufacturerService> _logger;

    public ManufacturerService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ManufacturerService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ManufacturerDto>> CreateManufacturerAsync(CreateManufacturerRequest request)
    {
        try
        {
            _logger.LogInformation("Criando fabricante: {Name}", request.Name);

            // Validar ano de fundação
            var currentYear = DateTime.UtcNow.Year;
            if (request.FoundedYear < 1800 || request.FoundedYear > currentYear)
            {
                return Result<ManufacturerDto>.Failure($"O ano de fundação deve estar entre 1800 e {currentYear}");
            }

            // Verificar se já existe fabricante com o mesmo nome
            var existingManufacturer = await _unitOfWork.Manufacturers
                .FindAsync(m => m.Name.ToLower() == request.Name.ToLower());

            if (existingManufacturer.Any())
            {
                _logger.LogWarning("Tentativa de criar fabricante duplicado: {Name}", request.Name);
                return Result<ManufacturerDto>.Failure("Já existe um fabricante com este nome");
            }

            // Criar novo fabricante
            var manufacturer = new Manufacturer
            {
                Name = request.Name.Trim(),
                Country = request.Country.Trim(),
                FoundedYear = request.FoundedYear,
                Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Manufacturers.AddAsync(manufacturer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersList);
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersActive);

            _logger.LogInformation("Fabricante criado com sucesso: {Id} - {Name}", manufacturer.Id, manufacturer.Name);

            var dto = MapToDto(manufacturer);
            return Result<ManufacturerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fabricante: {Name}", request.Name);
            return Result<ManufacturerDto>.Failure("Erro ao criar fabricante. Tente novamente.");
        }
    }

    public async Task<Result<ManufacturerDto>> UpdateManufacturerAsync(UpdateManufacturerRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando fabricante: {Id}", request.Id);

            var manufacturer = await _unitOfWork.Manufacturers.GetByIdAsync(request.Id);
            if (manufacturer == null)
            {
                _logger.LogWarning("Fabricante não encontrado: {Id}", request.Id);
                return Result<ManufacturerDto>.Failure("Fabricante não encontrado");
            }

            // Validar ano de fundação
            var currentYear = DateTime.UtcNow.Year;
            if (request.FoundedYear < 1800 || request.FoundedYear > currentYear)
            {
                return Result<ManufacturerDto>.Failure($"O ano de fundação deve estar entre 1800 e {currentYear}");
            }

            // Verificar se já existe outro fabricante com o mesmo nome
            var existingManufacturer = await _unitOfWork.Manufacturers
                .FindAsync(m => m.Name.ToLower() == request.Name.ToLower() && m.Id != request.Id);

            if (existingManufacturer.Any())
            {
                _logger.LogWarning("Tentativa de atualizar para nome duplicado: {Name}", request.Name);
                return Result<ManufacturerDto>.Failure("Já existe outro fabricante com este nome");
            }

            // Atualizar dados
            manufacturer.Name = request.Name.Trim();
            manufacturer.Country = request.Country.Trim();
            manufacturer.FoundedYear = request.FoundedYear;
            manufacturer.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();

            _unitOfWork.Manufacturers.Update(manufacturer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersList);
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersActive);
            await _cacheService.RemoveAsync(CacheKeys.VehiclesByManufacturer(manufacturer.Id));

            _logger.LogInformation("Fabricante atualizado com sucesso: {Id} - {Name}", manufacturer.Id, manufacturer.Name);

            var dto = MapToDto(manufacturer);
            return Result<ManufacturerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar fabricante: {Id}", request.Id);
            return Result<ManufacturerDto>.Failure("Erro ao atualizar fabricante. Tente novamente.");
        }
    }

    public async Task<Result<bool>> DeleteManufacturerAsync(int id)
    {
        try
        {
            _logger.LogInformation("Excluindo fabricante: {Id}", id);

            var manufacturer = await _unitOfWork.Manufacturers.GetByIdAsync(id);
            if (manufacturer == null)
            {
                _logger.LogWarning("Fabricante não encontrado para exclusão: {Id}", id);
                return Result<bool>.Failure("Fabricante não encontrado");
            }

            // Verificar se possui veículos associados
            var hasVehicles = await _unitOfWork.Vehicles
                .FindAsync(v => v.ManufacturerId == id);

            if (hasVehicles.Any())
            {
                _logger.LogWarning("Tentativa de excluir fabricante com veículos associados: {Id}", id);
                return Result<bool>.Failure("Não é possível excluir um fabricante que possui veículos cadastrados");
            }

            _unitOfWork.Manufacturers.Remove(manufacturer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersList);
            await _cacheService.RemoveAsync(CacheKeys.ManufacturersActive);

            _logger.LogInformation("Fabricante excluído com sucesso: {Id}", id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir fabricante: {Id}", id);
            return Result<bool>.Failure("Erro ao excluir fabricante. Tente novamente.");
        }
    }

    public async Task<Result<ManufacturerDto>> GetManufacturerByIdAsync(int id)
    {
        try
        {
            var manufacturer = await _unitOfWork.Manufacturers.GetByIdAsync(id);
            if (manufacturer == null)
            {
                return Result<ManufacturerDto>.Failure("Fabricante não encontrado");
            }

            var dto = MapToDto(manufacturer);
            return Result<ManufacturerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fabricante: {Id}", id);
            return Result<ManufacturerDto>.Failure("Erro ao buscar fabricante");
        }
    }

    public async Task<Result<List<ManufacturerDto>>> GetAllManufacturersAsync()
    {
        try
        {
            var manufacturers = await _cacheService.GetOrCreateAsync(
                CacheKeys.ManufacturersList,
                async () => await _unitOfWork.Manufacturers.GetAllAsync(),
                TimeSpan.FromMinutes(30)
            );

            var dtos = manufacturers.Select(MapToDto).ToList();
            return Result<List<ManufacturerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os fabricantes");
            return Result<List<ManufacturerDto>>.Failure("Erro ao buscar fabricantes");
        }
    }

    public async Task<Result<List<ManufacturerDto>>> GetActiveManufacturersAsync()
    {
        try
        {
            var manufacturers = await _cacheService.GetOrCreateAsync(
                CacheKeys.ManufacturersActive,
                async () =>
                {
                    var all = await _unitOfWork.Manufacturers.GetAllAsync();
                    return all.Where(m => m.IsActive).ToList();
                },
                TimeSpan.FromMinutes(30)
            );

            var dtos = manufacturers.Select(MapToDto).ToList();
            return Result<List<ManufacturerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fabricantes ativos");
            return Result<List<ManufacturerDto>>.Failure("Erro ao buscar fabricantes ativos");
        }
    }

    private static ManufacturerDto MapToDto(Manufacturer manufacturer)
    {
        return new ManufacturerDto
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            Country = manufacturer.Country,
            FoundedYear = manufacturer.FoundedYear,
            Website = manufacturer.Website,
            IsActive = manufacturer.IsActive
        };
    }
}
