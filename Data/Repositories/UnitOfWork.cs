using Microsoft.EntityFrameworkCore.Storage;

namespace SunsetCars.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IManufacturerRepository? _manufacturers;
    private IVehicleRepository? _vehicles;
    private IDealershipRepository? _dealerships;
    private ICustomerRepository? _customers;
    private ISaleRepository? _sales;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IManufacturerRepository Manufacturers =>
        _manufacturers ??= new ManufacturerRepository(_context);

    public IVehicleRepository Vehicles =>
        _vehicles ??= new VehicleRepository(_context);

    public IDealershipRepository Dealerships =>
        _dealerships ??= new DealershipRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public ISaleRepository Sales =>
        _sales ??= new SaleRepository(_context);

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
