using SunsetCars.Models;

namespace SunsetCars.Data.Repositories;

public interface IManufacturerRepository : IRepository<Manufacturer>
{
}

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<List<Vehicle>> GetByManufacturerIdAsync(int manufacturerId);
    Task<List<Vehicle>> GetAvailableVehiclesAsync();
}

public interface IDealershipRepository : IRepository<Dealership>
{
    Task<Dealership?> GetByPostalCodeAsync(string postalCode);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByCpfAsync(string cpf);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByIdWithSalesAsync(int id);
}

public interface ISaleRepository : IRepository<Sale>
{
    Task<List<Sale>> GetSalesForUserAsync(string userId, string[] roles);
    Task<Sale?> GetByIdWithDetailsAsync(int id);
    Task<Sale?> GetByProtocolAsync(string protocol);
    Task<List<Sale>> GetMonthlySalesAsync(int year, int month);
    Task<List<Sale>> GetRecentSalesAsync(DateTime fromDate);
    Task<bool> ProtocolExistsAsync(string protocol);
    Task<List<Sale>> GetSalesByDealershipAsync(int dealershipId);
    Task<List<Sale>> GetSalesBySalesPersonAsync(string salesPersonId);
}
