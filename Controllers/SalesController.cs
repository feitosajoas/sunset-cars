using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Requests;
using SunsetCars.Services.Domain;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly ISaleService _saleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        ISaleService saleService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int id)
    {
        return View();
    }

    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Administrador,Gerente")]
    public IActionResult Edit(int id)
    {
        return View();
    }

    [Authorize(Roles = "Administrador,Gerente")]
    public IActionResult Delete(int id)
    {
        return View();
    }

    [HttpGet]
    [Route("api/sales")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllApi([FromQuery] string? searchString = null, [FromQuery] int? dealershipId = null, [FromQuery] int? customerId = null)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _saleService.GetSalesForUserAsync(currentUser.Id, currentUser.Roles);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao buscar vendas: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            var sales = result.Data ?? new List<DTOs.Responses.SaleDto>();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                sales = sales.Where(s =>
                    s.Protocol.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.VehicleModel.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.DealershipName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (dealershipId.HasValue)
            {
                sales = sales.Where(s => s.DealershipId == dealershipId.Value).ToList();
            }

            if (customerId.HasValue)
            {
                sales = sales.Where(s => s.CustomerId == customerId.Value).ToList();
            }

            var result_final = sales
                .OrderByDescending(s => s.SaleDate)
                .ThenBy(s => s.Protocol)
                .ToList();

            return Ok(result_final);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/sales/{id}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetByIdApi([FromRoute] int id)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _saleService.GetSaleByIdAsync(id, currentUser.Id, currentUser.Roles);

            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound(new { error = "Venda não encontrada" });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost]
    [Route("api/sales")]
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateApi([FromBody] CreateSaleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _saleService.CreateSaleAsync(request, currentUser.Id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao criar venda: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new 
                { 
                    error = result.ErrorMessage,
                    validationErrors = result.ValidationErrors 
                });
            }

            return CreatedAtAction(
                nameof(GetByIdApi), 
                new { id = result.Data!.Id }, 
                result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut]
    [Route("api/sales/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateApi([FromRoute] int id, [FromBody] UpdateSaleRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID da rota não confere com o ID do objeto" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _saleService.UpdateSaleAsync(id, request, currentUser.Id, currentUser.Roles);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao atualizar venda {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return BadRequest(new 
                { 
                    error = result.ErrorMessage,
                    validationErrors = result.ValidationErrors 
                });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar venda {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete]
    [Route("api/sales/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteApi([FromRoute] int id)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _saleService.DeleteSaleAsync(id, currentUser.Id, currentUser.Roles);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao excluir venda {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir venda {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/sales/dealership/{dealershipId}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetSalesByDealershipApi([FromRoute] int dealershipId)
    {
        try
        {
            // Verificar se a concessionária existe
            var dealership = await _unitOfWork.Dealerships.GetByIdAsync(dealershipId);
            if (dealership == null)
            {
                return NotFound(new { error = "Concessionária não encontrada" });
            }

            // Buscar vendas da concessionária
            var sales = await _unitOfWork.Sales.GetSalesByDealershipAsync(dealershipId);
            
            var salesDto = sales.Select(s => new
            {
                Id = s.Id,
                Protocol = s.Protocol,
                SaleDate = s.SaleDate,
                SalePrice = s.SalePrice,
                CustomerName = s.Customer?.Name,
                VehicleModel = s.Vehicle?.Model,
                VehicleYear = s.Vehicle?.Year,
                CreatedAt = s.CreatedAt
            }).OrderByDescending(s => s.SaleDate);

            return Ok(salesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas da concessionária {DealershipId}", dealershipId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/sales/customer/{customerId}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetSalesByCustomerApi([FromRoute] int customerId)
    {
        try
        {
            // Verificar se o cliente existe
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { error = "Cliente não encontrado" });
            }

            // Buscar vendas do cliente usando repositório direto
            var sales = await _unitOfWork.Sales.GetActiveAsync();
            var customerSales = sales.Where(s => s.CustomerId == customerId).ToList();
            
            var salesDto = customerSales.Select(s => new
            {
                Id = s.Id,
                Protocol = s.Protocol,
                SaleDate = s.SaleDate,
                SalePrice = s.SalePrice,
                DealershipName = s.Dealership?.Name,
                VehicleModel = s.Vehicle?.Model,
                VehicleYear = s.Vehicle?.Year,
                CreatedAt = s.CreatedAt
            }).OrderByDescending(s => s.SaleDate);

            return Ok(salesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas do cliente {CustomerId}", customerId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/sales/statistics")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> GetSalesStatisticsApi()
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var salesResult = await _saleService.GetSalesForUserAsync(currentUser.Id, currentUser.Roles);

            if (!salesResult.IsSuccess)
            {
                return BadRequest(new { error = salesResult.ErrorMessage });
            }

            var sales = salesResult.Data ?? new List<DTOs.Responses.SaleDto>();

            var statistics = new
            {
                TotalSales = sales.Count,
                TotalRevenue = sales.Sum(s => s.SalePrice),
                AveragePrice = sales.Any() ? sales.Average(s => s.SalePrice) : 0,
                SalesToday = sales.Count(s => s.SaleDate.Date == DateTime.Today),
                SalesThisMonth = sales.Count(s => s.SaleDate.Month == DateTime.Now.Month && s.SaleDate.Year == DateTime.Now.Year),
                SalesThisYear = sales.Count(s => s.SaleDate.Year == DateTime.Now.Year),
                TopDealerships = sales.GroupBy(s => new { s.DealershipId, s.DealershipName })
                    .Select(g => new { DealershipName = g.Key.DealershipName, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList(),
                RecentSales = sales.OrderByDescending(s => s.SaleDate)
                    .Take(10)
                    .Select(s => new { s.Id, s.Protocol, s.CustomerName, s.SalePrice, s.SaleDate })
                    .ToList()
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas de vendas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

}
