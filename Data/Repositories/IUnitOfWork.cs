using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace SunsetCars.Data.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetActiveAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(int id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
}

public interface IUnitOfWork : IDisposable
{
    IManufacturerRepository Manufacturers { get; }
    IVehicleRepository Vehicles { get; }
    IDealershipRepository Dealerships { get; }
    ICustomerRepository Customers { get; }
    ISaleRepository Sales { get; }

    Task<int> CommitAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
