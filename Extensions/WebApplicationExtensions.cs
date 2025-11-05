using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SunsetCars.Data;
using SunsetCars.Models;

namespace SunsetCars.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting database initialization...");

            // Apply migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed data
            await SeedDataAsync(roleManager, userManager, logger);
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }

        return app;
    }

    private static async Task SeedDataAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // Seed roles
        string[] roles = { "Administrador", "Gerente", "Vendedor" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role created: {Role}", role);
            }
        }

        // Seed admin user
        var adminEmail = "admin@sunsetcars.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador do Sistema",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                logger.LogInformation("Admin user created: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed gerente user
        var gerenteEmail = "gerente@sunsetcars.com";
        var gerenteUser = await userManager.FindByEmailAsync(gerenteEmail);

        if (gerenteUser == null)
        {
            gerenteUser = new ApplicationUser
            {
                UserName = gerenteEmail,
                Email = gerenteEmail,
                FullName = "Gerente de Vendas",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(gerenteUser, "Gerente@123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(gerenteUser, "Gerente");
                logger.LogInformation("Manager user created: {Email}", gerenteEmail);
            }
        }

        // Seed vendedor user
        var vendedorEmail = "vendedor@sunsetcars.com";
        var vendedorUser = await userManager.FindByEmailAsync(vendedorEmail);

        if (vendedorUser == null)
        {
            vendedorUser = new ApplicationUser
            {
                UserName = vendedorEmail,
                Email = vendedorEmail,
                FullName = "Vendedor Teste",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(vendedorUser, "Vendedor@123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(vendedorUser, "Vendedor");
                logger.LogInformation("Salesperson user created: {Email}", vendedorEmail);
            }
        }
    }
}
