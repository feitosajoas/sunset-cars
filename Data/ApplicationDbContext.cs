using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SunsetCars.Models;

namespace SunsetCars.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Dealership> Dealerships { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurações específicas
        builder.Entity<Manufacturer>()
            .HasIndex(m => m.Name)
            .IsUnique();

        builder.Entity<Dealership>()
            .HasIndex(d => d.Name)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(c => c.CPF)
            .IsUnique();

        builder.Entity<Sale>()
            .HasIndex(s => s.Protocol)
            .IsUnique();

        // Configurar relacionamentos
        builder.Entity<Vehicle>()
            .HasOne(v => v.Manufacturer)
            .WithMany(m => m.Vehicles)
            .HasForeignKey(v => v.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Vehicle)
            .WithMany(v => v.Sales)
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Dealership)
            .WithMany(d => d.Sales)
            .HasForeignKey(s => s.DealershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed inicial de dados
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        var seedDate = new DateTime(2024, 1, 1);

        // Seed de fabricantes
        builder.Entity<Manufacturer>().HasData(
            new Manufacturer { Id = 1, Name = "Toyota", Country = "Japão", FoundedYear = 1937, Website = "https://www.toyota.com", CreatedAt = seedDate, IsActive = true },
            new Manufacturer { Id = 2, Name = "Volkswagen", Country = "Alemanha", FoundedYear = 1937, Website = "https://www.volkswagen.com", CreatedAt = seedDate, IsActive = true },
            new Manufacturer { Id = 3, Name = "Ford", Country = "Estados Unidos", FoundedYear = 1903, Website = "https://www.ford.com", CreatedAt = seedDate, IsActive = true },
            new Manufacturer { Id = 4, Name = "Chevrolet", Country = "Estados Unidos", FoundedYear = 1911, Website = "https://www.chevrolet.com", CreatedAt = seedDate, IsActive = true },
            new Manufacturer { Id = 5, Name = "Honda", Country = "Japão", FoundedYear = 1948, Website = "https://www.honda.com", CreatedAt = seedDate, IsActive = true }
        );

        // Seed de concessionárias
        builder.Entity<Dealership>().HasData(
            new Dealership
            {
                Id = 1,
                Name = "Sunset Motors Centro",
                Address = "Rua das Flores, 123",
                City = "São Paulo",
                State = "São Paulo",
                ZipCode = "01234-567",
                Phone = "(11) 1234-5678",
                Email = "centro@sunsetmotors.com.br",
                MaxCapacity = 50,
                CreatedAt = seedDate,
                IsActive = true
            },
            new Dealership
            {
                Id = 2,
                Name = "Sunset Motors Sul",
                Address = "Av. Paulista, 456",
                City = "São Paulo",
                State = "São Paulo",
                ZipCode = "04567-890",
                Phone = "(11) 9876-5432",
                Email = "sul@sunsetmotors.com.br",
                MaxCapacity = 75,
                CreatedAt = seedDate,
                IsActive = true
            }
        );
    }
}
