namespace SunsetCars.Services.Infrastructure;

/// <summary>
/// Centraliza todas as chaves de cache usadas na aplicação
/// </summary>
public static class CacheKeys
{
    // Manufacturers
    public const string ManufacturersList = "manufacturers:all";
    public const string ManufacturersActive = "manufacturers:active";
    public static string ManufacturerById(int id) => $"manufacturer:{id}";

    // Vehicles
    public const string VehiclesList = "vehicles:all";
    public const string VehiclesActive = "vehicles:active";
    public static string VehicleById(int id) => $"vehicle:{id}";
    public static string VehiclesByManufacturer(int manufacturerId) => $"vehicles:manufacturer:{manufacturerId}";

    // Dealerships
    public const string DealershipsList = "dealerships:all";
    public const string DealershipsActive = "dealerships:active";
    public static string DealershipById(int id) => $"dealership:{id}";
    public static string DealershipByZipCode(string zipCode) => $"dealership:zipcode:{zipCode}";

    // Customers
    public const string CustomersList = "customers:all";
    public const string CustomersActive = "customers:active";
    public static string CustomerById(int id) => $"customer:{id}";
    public static string CustomerByCpf(string cpf) => $"customer:cpf:{cpf}";

    // Sales
    public static string SaleById(int id) => $"sale:{id}";
    public static string SaleByProtocol(string protocol) => $"sale:protocol:{protocol}";
    public static string SalesByDealership(int dealershipId) => $"sales:dealership:{dealershipId}";
    public static string SalesByUser(string userId) => $"sales:user:{userId}";

    // Dashboard
    public const string DashboardData = "dashboard:data";
    public static string MonthlySalesReport(int year, int month) => $"report:monthly:{year}:{month}";

    // Prefixes para invalidação em massa
    public const string ManufacturersPrefix = "manufacturers:";
    public const string VehiclesPrefix = "vehicles:";
    public const string DealershipsPrefix = "dealerships:";
    public const string CustomersPrefix = "customers:";
    public const string SalesPrefix = "sales:";
    public const string ReportsPrefix = "report:";
}
