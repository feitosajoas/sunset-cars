using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Common;
using SunsetCars.DTOs.Requests;
using SunsetCars.DTOs.Responses;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;
using System.Text.RegularExpressions;

namespace SunsetCars.Services.Domain;

public interface ICustomerService
{
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Result<CustomerDto>> UpdateCustomerAsync(UpdateCustomerRequest request);
    Task<Result<bool>> DeleteCustomerAsync(int id);
    Task<Result<CustomerDto>> GetCustomerByIdAsync(int id);
    Task<Result<List<CustomerDto>>> GetAllCustomersAsync();
    Task<Result<CustomerDto>> GetCustomerByCpfAsync(string cpf);
}

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            _logger.LogInformation("Criando cliente: {Name}", request.Name);

            // Limpar e validar CPF
            var cpfCleaned = CleanCpf(request.CPF);
            if (!IsValidCpf(cpfCleaned))
            {
                return Result<CustomerDto>.Failure("CPF inválido");
            }

            // Verificar se já existe cliente com o mesmo CPF
            var existingByCpf = await _unitOfWork.Customers.GetByCpfAsync(cpfCleaned);
            if (existingByCpf != null)
            {
                _logger.LogWarning("Tentativa de criar cliente com CPF duplicado: {CPF}", cpfCleaned);
                return Result<CustomerDto>.Failure("Já existe um cliente com este CPF");
            }

            // Criar novo cliente
            var customer = new Customer
            {
                Name = request.Name.Trim(),
                CPF = cpfCleaned,
                Phone = request.Phone.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.CustomersList);

            _logger.LogInformation("Cliente criado com sucesso: {Id} - {Name}", customer.Id, customer.Name);

            var dto = MapToDto(customer);
            return Result<CustomerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente: {Name}", request.Name);
            return Result<CustomerDto>.Failure("Erro ao criar cliente. Tente novamente.");
        }
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando cliente: {Id}", request.Id);

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
            if (customer == null)
            {
                _logger.LogWarning("Cliente não encontrado: {Id}", request.Id);
                return Result<CustomerDto>.Failure("Cliente não encontrado");
            }

            // Limpar e validar CPF
            var cpfCleaned = CleanCpf(request.CPF);
            if (!IsValidCpf(cpfCleaned))
            {
                return Result<CustomerDto>.Failure("CPF inválido");
            }

            // Verificar se já existe outro cliente com o mesmo CPF
            var existingByCpf = await _unitOfWork.Customers.GetByCpfAsync(cpfCleaned);
            if (existingByCpf != null && existingByCpf.Id != request.Id)
            {
                _logger.LogWarning("Tentativa de atualizar para CPF duplicado: {CPF}", cpfCleaned);
                return Result<CustomerDto>.Failure("Já existe outro cliente com este CPF");
            }

            // Atualizar dados
            customer.Name = request.Name.Trim();
            customer.CPF = cpfCleaned;
            customer.Phone = request.Phone.Trim();

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.CustomersList);

            _logger.LogInformation("Cliente atualizado com sucesso: {Id} - {Name}", customer.Id, customer.Name);

            var dto = MapToDto(customer);
            return Result<CustomerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente: {Id}", request.Id);
            return Result<CustomerDto>.Failure("Erro ao atualizar cliente. Tente novamente.");
        }
    }

    public async Task<Result<bool>> DeleteCustomerAsync(int id)
    {
        try
        {
            _logger.LogInformation("Excluindo cliente: {Id}", id);

            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Cliente não encontrado para exclusão: {Id}", id);
                return Result<bool>.Failure("Cliente não encontrado");
            }

            // Verificar se possui vendas associadas
            var hasSales = await _unitOfWork.Sales
                .FindAsync(s => s.CustomerId == id);

            if (hasSales.Any())
            {
                _logger.LogWarning("Tentativa de excluir cliente com vendas associadas: {Id}", id);
                return Result<bool>.Failure("Não é possível excluir um cliente que possui vendas registradas");
            }

            _unitOfWork.Customers.Remove(customer);
            await _unitOfWork.CommitAsync();

            // Invalidar cache
            await _cacheService.RemoveAsync(CacheKeys.CustomersList);

            _logger.LogInformation("Cliente excluído com sucesso: {Id}", id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente: {Id}", id);
            return Result<bool>.Failure("Erro ao excluir cliente. Tente novamente.");
        }
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(int id)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                return Result<CustomerDto>.Failure("Cliente não encontrado");
            }

            var dto = MapToDto(customer);
            return Result<CustomerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente: {Id}", id);
            return Result<CustomerDto>.Failure("Erro ao buscar cliente");
        }
    }

    public async Task<Result<List<CustomerDto>>> GetAllCustomersAsync()
    {
        try
        {
            var customers = await _cacheService.GetOrCreateAsync(
                CacheKeys.CustomersList,
                async () => await _unitOfWork.Customers.GetAllAsync(),
                TimeSpan.FromMinutes(30)
            );

            var dtos = customers.Select(MapToDto).ToList();
            return Result<List<CustomerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os clientes");
            return Result<List<CustomerDto>>.Failure("Erro ao buscar clientes");
        }
    }

    public async Task<Result<CustomerDto>> GetCustomerByCpfAsync(string cpf)
    {
        try
        {
            var cpfCleaned = CleanCpf(cpf);
            var customer = await _unitOfWork.Customers.GetByCpfAsync(cpfCleaned);
            if (customer == null)
            {
                return Result<CustomerDto>.Failure("Cliente não encontrado");
            }

            var dto = MapToDto(customer);
            return Result<CustomerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por CPF: {CPF}", cpf);
            return Result<CustomerDto>.Failure("Erro ao buscar cliente");
        }
    }

    private static string CleanCpf(string cpf)
    {
        return Regex.Replace(cpf, @"[^\d]", "");
    }

    private static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
            return false;

        // CPFs conhecidos como inválidos
        if (cpf == "00000000000" || cpf == "11111111111" || cpf == "22222222222" ||
            cpf == "33333333333" || cpf == "44444444444" || cpf == "55555555555" ||
            cpf == "66666666666" || cpf == "77777777777" || cpf == "88888888888" ||
            cpf == "99999999999")
            return false;

        // Validação dos dígitos verificadores
        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf[..9];
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        string digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Cpf = customer.CPF,
            Phone = customer.Phone,
            Email = "", // Customer não tem campo Email no modelo atual
            Address = "", // Customer não tem campo Address no modelo atual
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };
    }
}
