using Microsoft.EntityFrameworkCore;
using SunsetCars.Data.Repositories;
using SunsetCars.Models;
using SunsetCars.Services.Infrastructure;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;

namespace SunsetCars.Services
{
    public interface IReportService
    {
        Task<SalesReportData> GetMonthlySalesReportAsync(int year, int month);
        Task<byte[]> GenerateSalesReportPdfAsync(SalesReportData reportData);
        Task<byte[]> GenerateSalesReportExcelAsync(SalesReportData reportData);
        Task<DashboardData> GetDashboardDataAsync();
    }

    public class SalesReportData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<SalesByType> SalesByVehicleType { get; set; } = new();
        public List<SalesByDealership> SalesByDealership { get; set; } = new();
        public List<SalesByManufacturer> SalesByManufacturer { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
    }

    public class SalesByType
    {
        public VehicleType VehicleType { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesByDealership
    {
        public string DealershipName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesByManufacturer
    {
        public string ManufacturerName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardData
    {
        public int TotalSalesThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<MonthlySales> MonthlySalesChart { get; set; } = new();
        public List<SalesByType> SalesByTypeChart { get; set; } = new();
        public List<TopDealership> TopDealerships { get; set; } = new();
    }

    public class MonthlySales
    {
        public string Month { get; set; } = string.Empty;
        public int Sales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopDealership
    {
        public string Name { get; set; } = string.Empty;
        public int Sales { get; set; }
    }

    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;
        private readonly ICacheService _cacheService;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<SalesReportData> GetMonthlySalesReportAsync(int year, int month)
        {
            // Cache monthly reports for 1 hour (historical data doesn't change much)
            return await _cacheService.GetOrCreateSingleAsync(
                CacheKeys.MonthlySalesReport(year, month),
                async () => await GenerateMonthlySalesReportAsync(year, month),
                TimeSpan.FromHours(1)
            ) ?? new SalesReportData { Year = year, Month = month };
        }

        private async Task<SalesReportData> GenerateMonthlySalesReportAsync(int year, int month)
        {
            var sales = await _unitOfWork.Sales.GetMonthlySalesAsync(year, month);

            var reportData = new SalesReportData
            {
                Year = year,
                Month = month,
                TotalSales = sales.Count,
                TotalRevenue = sales.Sum(s => s.SalePrice)
            };

            // Sales by vehicle type
            reportData.SalesByVehicleType = sales
                .Where(s => s.Vehicle != null)
                .GroupBy(s => s.Vehicle!.Type)
                .Select(g => new SalesByType
                {
                    VehicleType = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.SalePrice)
                })
                .ToList();

            // Sales by dealership
            reportData.SalesByDealership = sales
                .Where(s => s.Dealership != null)
                .GroupBy(s => s.Dealership!.Name)
                .Select(g => new SalesByDealership
                {
                    DealershipName = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.SalePrice)
                })
                .ToList();

            // Sales by manufacturer
            reportData.SalesByManufacturer = sales
                .Where(s => s.Vehicle?.Manufacturer != null)
                .GroupBy(s => s.Vehicle!.Manufacturer!.Name)
                .Select(g => new SalesByManufacturer
                {
                    ManufacturerName = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.SalePrice)
                })
                .ToList();

            return reportData;
        }

        public Task<byte[]> GenerateSalesReportPdfAsync(SalesReportData reportData)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4);
            PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph($"Relatório de Vendas - {reportData.Month:00}/{reportData.Year}", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            document.Add(new Paragraph(" "));

            // Summary
            var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            document.Add(new Paragraph($"Total de Vendas: {reportData.TotalSales}", summaryFont));
            document.Add(new Paragraph($"Receita Total: {reportData.TotalRevenue:C}", summaryFont));

            document.Add(new Paragraph(" "));

            // Sales by type table
            var table = new PdfPTable(3);
            table.AddCell("Tipo de Veículo");
            table.AddCell("Quantidade");
            table.AddCell("Receita");

            foreach (var item in reportData.SalesByVehicleType)
            {
                table.AddCell(item.VehicleType.ToString());
                table.AddCell(item.Count.ToString());
                table.AddCell(item.Revenue.ToString("C"));
            }

            document.Add(table);
            document.Close();

            return Task.FromResult(memoryStream.ToArray());
        }

        public Task<byte[]> GenerateSalesReportExcelAsync(SalesReportData reportData)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Relatório de Vendas");

            // Header
            worksheet.Cells["A1"].Value = $"Relatório de Vendas - {reportData.Month:00}/{reportData.Year}";
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            // Summary
            worksheet.Cells["A3"].Value = "Total de Vendas:";
            worksheet.Cells["B3"].Value = reportData.TotalSales;
            worksheet.Cells["A4"].Value = "Receita Total:";
            worksheet.Cells["B4"].Value = reportData.TotalRevenue;
            worksheet.Cells["B4"].Style.Numberformat.Format = "R$ #,##0.00";

            // Sales by type
            worksheet.Cells["A6"].Value = "Vendas por Tipo de Veículo";
            worksheet.Cells["A6"].Style.Font.Bold = true;
            worksheet.Cells["A7"].Value = "Tipo";
            worksheet.Cells["B7"].Value = "Quantidade";
            worksheet.Cells["C7"].Value = "Receita";

            int row = 8;
            foreach (var item in reportData.SalesByVehicleType)
            {
                worksheet.Cells[$"A{row}"].Value = item.VehicleType.ToString();
                worksheet.Cells[$"B{row}"].Value = item.Count;
                worksheet.Cells[$"C{row}"].Value = item.Revenue;
                worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "R$ #,##0.00";
                row++;
            }

            return Task.FromResult(package.GetAsByteArray());
        }

        public async Task<DashboardData> GetDashboardDataAsync()
        {
            // Cache dashboard data for 5 minutes (dashboard changes frequently with new sales)
            return await _cacheService.GetOrCreateSingleAsync(
                CacheKeys.DashboardData,
                async () => await GenerateDashboardDataAsync(),
                TimeSpan.FromMinutes(5)
            ) ?? new DashboardData();
        }

        private async Task<DashboardData> GenerateDashboardDataAsync()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Get sales from the last 6 months in a single optimized query
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5).Date;
            var allRecentSales = await _unitOfWork.Sales.GetRecentSalesAsync(sixMonthsAgo);

            // Current month sales
            var salesThisMonth = allRecentSales
                .Where(s => s.SaleDate.Year == currentYear && s.SaleDate.Month == currentMonth)
                .ToList();

            var dashboardData = new DashboardData
            {
                TotalSalesThisMonth = salesThisMonth.Count,
                RevenueThisMonth = salesThisMonth.Sum(s => s.SalePrice)
            };

            // Monthly sales for the last 6 months
            var monthlySales = new List<MonthlySales>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var sales = allRecentSales
                    .Where(s => s.SaleDate.Year == date.Year && s.SaleDate.Month == date.Month)
                    .ToList();

                monthlySales.Add(new MonthlySales
                {
                    Month = date.ToString("MMM/yyyy"),
                    Sales = sales.Count,
                    Revenue = sales.Sum(s => s.SalePrice)
                });
            }
            dashboardData.MonthlySalesChart = monthlySales;

            // Sales by type
            var salesByType = salesThisMonth
                .GroupBy(s => s.Vehicle?.Type ?? VehicleType.Car)
                .Select(g => new SalesByType
                {
                    VehicleType = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.SalePrice)
                })
                .ToList();

            dashboardData.SalesByTypeChart = salesByType;

            // Top dealerships
            var topDealerships = salesThisMonth
                .GroupBy(s => s.Dealership?.Name ?? "Desconhecido")
                .Select(g => new TopDealership
                {
                    Name = g.Key,
                    Sales = g.Count()
                })
                .OrderByDescending(x => x.Sales)
                .Take(5)
                .ToList();

            dashboardData.TopDealerships = topDealerships;

            return dashboardData;
        }
    }
}