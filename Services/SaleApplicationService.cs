using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SunsetCars.Data;
using SunsetCars.Models;
using System.Security.Claims;

namespace Application.Services
{
    public interface ISaleApplicationService
    {
        Task<Sale> CreateSaleAsync(CreateSaleCommand command);
        Task<Sale> UpdateSaleAsync(UpdateSaleCommand command);
        Task<IEnumerable<Sale>> GetSalesForUserAsync(string userId, IEnumerable<string> userRoles);
        Task<Sale?> GetSaleByIdAsync(int id, string userId, IEnumerable<string> userRoles);
        Task DeleteSaleAsync(int id);
    }

    public interface IUserService
    {
        Task<CurrentUserInfo> GetCurrentUserAsync(ClaimsPrincipal user);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    }

    public class CreateSaleCommand
    {
        public int DealershipId { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public decimal SalePrice { get; set; }
        public string SalesPersonId { get; set; } = string.Empty;
    }

    public class UpdateSaleCommand
    {
        public int Id { get; set; }
        public int DealershipId { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime SaleDate { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string SalesPersonId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CurrentUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    public class SaleApplicationService : ISaleApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaleApplicationService> _logger;

        public SaleApplicationService(ApplicationDbContext context, ILogger<SaleApplicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Sale> CreateSaleAsync(CreateSaleCommand command)
        {
            _logger.LogInformation("Creating sale for vehicle {VehicleId}", command.VehicleId);

            var vehicle = await _context.Vehicles.FindAsync(command.VehicleId);
            if (vehicle == null)
                throw new ArgumentException($"Veículo com ID {command.VehicleId} não encontrado");

            var customer = await _context.Customers.FindAsync(command.CustomerId);
            if (customer == null)
                throw new ArgumentException($"Cliente com ID {command.CustomerId} não encontrado");

            // Business rule validations
            if (command.SalePrice > vehicle.Price)
                throw new InvalidOperationException("Preço de venda não pode ser maior que o preço do veículo");

            // Generate protocol
            var protocol = await GenerateUniqueProtocolAsync();
            
            var sale = new Sale
            {
                DealershipId = command.DealershipId,
                VehicleId = command.VehicleId,
                CustomerId = command.CustomerId,
                SalePrice = command.SalePrice,
                SalesPersonId = command.SalesPersonId,
                Protocol = protocol,
                SaleDate = DateTime.Today,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sale created successfully with protocol {Protocol}", sale.Protocol);

            return sale;
        }

        public async Task<Sale> UpdateSaleAsync(UpdateSaleCommand command)
        {
            _logger.LogInformation("Updating sale with ID {SaleId}", command.Id);

            var sale = await _context.Sales.FindAsync(command.Id);
            if (sale == null)
                throw new ArgumentException($"Venda com ID {command.Id} não encontrada");

            var vehicle = await _context.Vehicles.FindAsync(command.VehicleId);
            if (vehicle == null)
                throw new ArgumentException($"Veículo com ID {command.VehicleId} não encontrado");

            // Business rule validations
            if (command.SalePrice > vehicle.Price)
                throw new InvalidOperationException("Preço de venda não pode ser maior que o preço do veículo");

            if (command.SaleDate > DateTime.Today)
                throw new InvalidOperationException("Data da venda não pode ser futura");

            // Update properties
            sale.DealershipId = command.DealershipId;
            sale.VehicleId = command.VehicleId;
            sale.CustomerId = command.CustomerId;
            sale.SalePrice = command.SalePrice;
            sale.SaleDate = command.SaleDate;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sale updated successfully with ID {SaleId}", sale.Id);

            return sale;
        }

        public async Task<IEnumerable<Sale>> GetSalesForUserAsync(string userId, IEnumerable<string> userRoles)
        {
            IQueryable<Sale> query = _context.Sales
                .Include(s => s.Dealership)
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v!.Manufacturer)
                .Include(s => s.Customer)
                .Include(s => s.SalesPerson)
                .Where(s => s.IsActive);

            // Vendedores só veem suas próprias vendas
            if (userRoles.Contains("Vendedor") && 
                !userRoles.Contains("Gerente") && 
                !userRoles.Contains("Administrador"))
            {
                query = query.Where(s => s.SalesPersonId == userId);
            }

            return await query.OrderByDescending(s => s.SaleDate).ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id, string userId, IEnumerable<string> userRoles)
        {
            var sale = await _context.Sales
                .Include(s => s.Dealership)
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v!.Manufacturer)
                .Include(s => s.Customer)
                .Include(s => s.SalesPerson)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return null;

            // Check permissions
            if (userRoles.Contains("Vendedor") && 
                !userRoles.Contains("Gerente") && 
                !userRoles.Contains("Administrador") &&
                sale.SalesPersonId != userId)
            {
                throw new UnauthorizedAccessException("Usuário não tem permissão para visualizar esta venda");
            }

            return sale;
        }

        public async Task DeleteSaleAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
                throw new ArgumentException($"Venda com ID {id} não encontrada");

            sale.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sale deactivated with ID {SaleId}", id);
        }

        private async Task<string> GenerateUniqueProtocolAsync()
        {
            string protocol;
            bool exists;

            do
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var random = new Random().Next(1000, 9999);
                protocol = $"SC-{date}-{random}";

                exists = await _context.Sales.AnyAsync(s => s.Protocol == protocol);
            }
            while (exists);

            return protocol;
        }
    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CurrentUserInfo> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var applicationUser = await _userManager.GetUserAsync(user);
            if (applicationUser == null)
                throw new UnauthorizedAccessException("Usuário não encontrado");

            var roles = await _userManager.GetRolesAsync(applicationUser);

            return new CurrentUserInfo
            {
                Id = applicationUser.Id,
                UserName = applicationUser.UserName ?? "",
                Roles = roles
            };
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<string>();

            return await _userManager.GetRolesAsync(user);
        }
    }
}