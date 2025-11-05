using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Services;

namespace SunsetCars.Controllers
{
    [Authorize(Roles = "Administrador,Gerente")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/Monthly
        public IActionResult Monthly()
        {
            var currentDate = DateTime.Now;
            ViewBag.Year = currentDate.Year;
            ViewBag.Month = currentDate.Month;
            return View();
        }

        // POST: Reports/Monthly
        [HttpPost]
        public async Task<IActionResult> Monthly(int year, int month)
        {
            if (year < 2020 || year > DateTime.Now.Year)
            {
                ModelState.AddModelError("", "Ano inválido");
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View();
            }

            if (month < 1 || month > 12)
            {
                ModelState.AddModelError("", "Mês inválido");
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View();
            }

            var reportData = await _reportService.GetMonthlySalesReportAsync(year, month);
            ViewBag.Year = year;
            ViewBag.Month = month;

            return View("MonthlyResults", reportData);
        }

        // GET: Reports/ExportPdf
        public async Task<IActionResult> ExportPdf(int year, int month)
        {
            var reportData = await _reportService.GetMonthlySalesReportAsync(year, month);
            var pdfBytes = await _reportService.GenerateSalesReportPdfAsync(reportData);

            return File(pdfBytes, "application/pdf", $"RelatorioVendas_{year}_{month:00}.pdf");
        }

        // GET: Reports/ExportExcel
        public async Task<IActionResult> ExportExcel(int year, int month)
        {
            var reportData = await _reportService.GetMonthlySalesReportAsync(year, month);
            var excelBytes = await _reportService.GenerateSalesReportExcelAsync(reportData);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       $"RelatorioVendas_{year}_{month:00}.xlsx");
        }

        // API endpoint para dados do gráfico de vendas mensais
        [HttpGet]
        public async Task<IActionResult> GetMonthlySalesData()
        {
            var dashboardData = await _reportService.GetDashboardDataAsync();

            var chartData = new
            {
                labels = dashboardData.MonthlySalesChart.Select(x => x.Month).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Vendas",
                        data = dashboardData.MonthlySalesChart.Select(x => x.Sales).ToArray(),
                        backgroundColor = "rgba(54, 162, 235, 0.6)",
                        borderColor = "rgba(54, 162, 235, 1)",
                        borderWidth = 1
                    }
                }
            };

            return Json(chartData);
        }

        // API endpoint para dados do gráfico de vendas por tipo
        [HttpGet]
        public async Task<IActionResult> GetSalesByTypeData()
        {
            var dashboardData = await _reportService.GetDashboardDataAsync();

            var chartData = new
            {
                labels = dashboardData.SalesByTypeChart.Select(x => x.VehicleType.ToString()).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = dashboardData.SalesByTypeChart.Select(x => x.Count).ToArray(),
                        backgroundColor = new[]
                        {
                            "rgba(255, 99, 132, 0.6)",
                            "rgba(54, 162, 235, 0.6)",
                            "rgba(255, 205, 86, 0.6)",
                            "rgba(75, 192, 192, 0.6)",
                            "rgba(153, 102, 255, 0.6)",
                            "rgba(255, 159, 64, 0.6)"
                        },
                        borderColor = new[]
                        {
                            "rgba(255, 99, 132, 1)",
                            "rgba(54, 162, 235, 1)",
                            "rgba(255, 205, 86, 1)",
                            "rgba(75, 192, 192, 1)",
                            "rgba(153, 102, 255, 1)",
                            "rgba(255, 159, 64, 1)"
                        },
                        borderWidth = 1
                    }
                }
            };

            return Json(chartData);
        }
    }
}