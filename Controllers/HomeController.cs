using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Models;
using SunsetCars.Services;

namespace SunsetCars.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReportService _reportService;

    public HomeController(ILogger<HomeController> logger, IReportService reportService)
    {
        _logger = logger;
        _reportService = reportService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var dashboardData = await _reportService.GetDashboardDataAsync();
        return View(dashboardData);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
