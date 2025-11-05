using Microsoft.EntityFrameworkCore;
using SunsetCars.Models;

namespace SunsetCars.Data.Repositories;

public class ManufacturerRepository : Repository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Manufacturer>> GetActiveAsync()
    {
        return await _dbSet
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Vehicle>> GetAllAsync()
    {
        return await _dbSet
            .Include(v => v.Manufacturer)
            .OrderBy(v => v.Model)
            .ToListAsync();
    }

    public override async Task<List<Vehicle>> GetActiveAsync()
    {
        return await _dbSet
            .Include(v => v.Manufacturer)
            .Where(v => v.IsActive)
            .OrderBy(v => v.Model)
            .ToListAsync();
    }

    public async Task<List<Vehicle>> GetByManufacturerIdAsync(int manufacturerId)
    {
        return await _dbSet
            .Include(v => v.Manufacturer)
            .Where(v => v.ManufacturerId == manufacturerId && v.IsActive)
            .OrderBy(v => v.Model)
            .ToListAsync();
    }

    public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
    {
        return await _dbSet
            .Include(v => v.Manufacturer)
            .Where(v => v.IsActive)
            .OrderBy(v => v.Manufacturer!.Name)
            .ThenBy(v => v.Model)
            .ToListAsync();
    }

    public override async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(v => v.Manufacturer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}

public class DealershipRepository : Repository<Dealership>, IDealershipRepository
{
    public DealershipRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Dealership>> GetActiveAsync()
    {
        return await _dbSet
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Dealership?> GetByPostalCodeAsync(string postalCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.ZipCode == postalCode && d.IsActive);
    }
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Customer>> GetActiveAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByCpfAsync(string cpf)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CPF == cpf);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Phone == email); // Customer não tem Email, usando Phone como fallback
    }

    public async Task<Customer?> GetByIdWithSalesAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Sales)
                .ThenInclude(s => s.Vehicle)
                    .ThenInclude(v => v!.Manufacturer)
            .Include(c => c.Sales)
                .ThenInclude(s => s.Dealership)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Sale>> GetActiveAsync()
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesForUserAsync(string userId, string[] roles)
    {
        var query = _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.IsActive);

        // Vendedores só veem suas próprias vendas
        if (roles.Contains("Vendedor") &&
            !roles.Contains("Gerente") &&
            !roles.Contains("Administrador"))
        {
            query = query.Where(s => s.SalesPersonId == userId);
        }

        return await query
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<Sale?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale?> GetByProtocolAsync(string protocol)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .FirstOrDefaultAsync(s => s.Protocol == protocol);
    }

    public async Task<List<Sale>> GetMonthlySalesAsync(int year, int month)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.IsActive && s.SaleDate.Year == year && s.SaleDate.Month == month)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetRecentSalesAsync(DateTime fromDate)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.IsActive && s.SaleDate >= fromDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<bool> ProtocolExistsAsync(string protocol)
    {
        return await _dbSet.AnyAsync(s => s.Protocol == protocol);
    }

    public async Task<List<Sale>> GetSalesByDealershipAsync(int dealershipId)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.DealershipId == dealershipId && s.IsActive)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesBySalesPersonAsync(string salesPersonId)
    {
        return await _dbSet
            .Include(s => s.Dealership)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v!.Manufacturer)
            .Include(s => s.Customer)
            .Include(s => s.SalesPerson)
            .Where(s => s.SalesPersonId == salesPersonId && s.IsActive)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
}
