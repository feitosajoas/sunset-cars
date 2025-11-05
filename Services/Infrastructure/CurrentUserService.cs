using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SunsetCars.DTOs;
using SunsetCars.Models;

namespace SunsetCars.Services.Infrastructure;

public interface ICurrentUserService
{
    Task<CurrentUserInfo> GetCurrentUserAsync();
    string GetUserId();
    Task<string[]> GetUserRolesAsync();
    Task<bool> IsInRoleAsync(string role);
    Task<bool> HasAnyRoleAsync(params string[] roles);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<CurrentUserInfo> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No user context available");

        var applicationUser = await _userManager.GetUserAsync(user)
            ?? throw new UnauthorizedAccessException("User not found");

        var roles = await _userManager.GetRolesAsync(applicationUser);

        return new CurrentUserInfo
        {
            Id = applicationUser.Id,
            UserName = applicationUser.UserName ?? "",
            Email = applicationUser.Email ?? "",
            FullName = applicationUser.FullName,
            Roles = roles.ToArray()
        };
    }

    public string GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            throw new UnauthorizedAccessException("User ID not found");
        }

        return userId;
    }

    public async Task<string[]> GetUserRolesAsync()
    {
        try
        {
            var userInfo = await GetCurrentUserAsync();
            return userInfo.Roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles");
            return Array.Empty<string>();
        }
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var roles = await GetUserRolesAsync();
        return roles.Contains(role);
    }

    public async Task<bool> HasAnyRoleAsync(params string[] roles)
    {
        var userRoles = await GetUserRolesAsync();
        return userRoles.Any(r => roles.Contains(r));
    }
}
