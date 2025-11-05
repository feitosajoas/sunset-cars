using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Requests;
using SunsetCars.Models;
using SunsetCars.Services.Domain;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Controllers;

[Authorize]
public class ManufacturersController : Controller
{
    private readonly IManufacturerService _manufacturerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManufacturersController> _logger;

    public ManufacturersController(
        IManufacturerService manufacturerService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<ManufacturersController> logger)
    {
        _manufacturerService = manufacturerService;
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

    [Authorize(Roles = "Administrador,Gerente")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Administrador,Gerente")]
    public IActionResult Edit(int id)
    {
        return View();
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        return View();
    }

    [HttpGet]
    [Route("api/manufacturers")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllApi([FromQuery] string? searchString = null, [FromQuery] string? country = null)
    {
        try
        {
            var result = await _manufacturerService.GetAllManufacturersAsync();

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao buscar fabricantes: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            var manufacturers = result.Data ?? new List<DTOs.Responses.ManufacturerDto>();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                manufacturers = manufacturers.Where(m =>
                    m.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    m.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(country))
            {
                manufacturers = manufacturers.Where(m => 
                    m.Country.Contains(country, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var result_final = manufacturers
                .OrderBy(m => m.Name)
                .ToList();

            return Ok(result_final);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fabricantes");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/manufacturers/{id}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetByIdApi([FromRoute] int id)
    {
        try
        {
            var result = await _manufacturerService.GetManufacturerByIdAsync(id);

            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound(new { error = "Fabricante não encontrado" });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fabricante {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost]
    [Route("api/manufacturers")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateApi([FromBody] CreateManufacturerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _manufacturerService.CreateManufacturerAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao criar fabricante: {ErrorMessage}", result.ErrorMessage);
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
            _logger.LogError(ex, "Erro ao criar fabricante");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut]
    [Route("api/manufacturers/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateApi([FromRoute] int id, [FromBody] UpdateManufacturerRequest request)
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
            var result = await _manufacturerService.UpdateManufacturerAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao atualizar fabricante {Id}: {ErrorMessage}", id, result.ErrorMessage);
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
            _logger.LogError(ex, "Erro ao atualizar fabricante {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete]
    [Route("api/manufacturers/{id}")]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteApi([FromRoute] int id)
    {
        try
        {
            var result = await _manufacturerService.DeleteManufacturerAsync(id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao excluir fabricante {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir fabricante {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/manufacturers/{id}/vehicles")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetManufacturerVehiclesApi([FromRoute] int id)
    {
        try
        {
            // Primeiro verificar se o fabricante existe
            var manufacturerResult = await _manufacturerService.GetManufacturerByIdAsync(id);
            if (!manufacturerResult.IsSuccess || manufacturerResult.Data == null)
            {
                return NotFound(new { error = "Fabricante não encontrado" });
            }

            // Buscar veículos do fabricante
            var vehicles = await _unitOfWork.Vehicles.GetByManufacturerIdAsync(id);
            
            var vehiclesDto = vehicles.Select(v => new
            {
                Id = v.Id,
                Model = v.Model,
                Year = v.Year,
                Price = v.Price,
                Type = (int)v.Type,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt
            }).OrderBy(v => v.Model);

            return Ok(vehiclesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar veículos do fabricante {ManufacturerId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}