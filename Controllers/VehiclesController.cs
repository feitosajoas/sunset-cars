using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Requests;
using SunsetCars.Models;
using SunsetCars.Services.Domain;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Controllers;

[Authorize]
public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        IVehicleService vehicleService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
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
    [Route("api/vehicles")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllApi([FromQuery] string? searchString = null, [FromQuery] int? manufacturerId = null, [FromQuery] VehicleType? type = null)
    {
        try
        {
            var result = await _vehicleService.GetActiveVehiclesAsync();

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao buscar veículos: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            var vehicles = result.Data ?? new List<DTOs.Responses.VehicleDto>();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v =>
                    v.Model.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    v.ManufacturerName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (manufacturerId.HasValue)
            {
                vehicles = vehicles.Where(v => v.ManufacturerId == manufacturerId.Value).ToList();
            }

            // Para filtro por tipo, precisamos buscar diretamente do banco
            if (type.HasValue)
            {
                var vehicleIds = (await _unitOfWork.Vehicles.GetActiveAsync())
                    .Where(v => v.Type == type.Value)
                    .Select(v => v.Id)
                    .ToList();
                
                vehicles = vehicles.Where(v => vehicleIds.Contains(v.Id)).ToList();
            }

            var result_final = vehicles
                .OrderBy(v => v.ManufacturerName)
                .ThenBy(v => v.Model)
                .ToList();

            return Ok(result_final);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar veículos");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/vehicles/{id}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetByIdApi([FromRoute] int id)
    {
        try
        {
            var result = await _vehicleService.GetVehicleByIdAsync(id);

            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound(new { error = "Veículo não encontrado" });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar veículo {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost]
    [Route("api/vehicles")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateApi([FromBody] CreateVehicleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _vehicleService.CreateVehicleAsync(request, currentUser.Id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao criar veículo: {ErrorMessage}", result.ErrorMessage);
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
            _logger.LogError(ex, "Erro ao criar veículo");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut]
    [Route("api/vehicles/{id}")]
    [Authorize(Roles = "Administrador,Gerente")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateApi([FromRoute] int id, [FromBody] UpdateVehicleRequest request)
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
            var result = await _vehicleService.UpdateVehicleAsync(id, request, currentUser.Id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao atualizar veículo {Id}: {ErrorMessage}", id, result.ErrorMessage);
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
            _logger.LogError(ex, "Erro ao atualizar veículo {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete]
    [Route("api/vehicles/{id}")]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteApi([FromRoute] int id)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var result = await _vehicleService.DeleteVehicleAsync(id, currentUser.Id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao excluir veículo {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir veículo {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [Route("api/vehicles/by-manufacturer/{manufacturerId}")]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetVehiclesByManufacturerApi([FromRoute] int manufacturerId)
    {
        try
        {
            var result = await _vehicleService.GetVehiclesByManufacturerAsync(manufacturerId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Erro ao buscar veículos por fabricante {ManufacturerId}: {ErrorMessage}", manufacturerId, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            var vehicles = result.Data!.Select(v => new
            {
                Id = v.Id,
                Text = $"{v.Model} ({v.Year})",
                Price = v.Price,
                Model = v.Model,
                Year = v.Year
            });

            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar veículos por fabricante {ManufacturerId}", manufacturerId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

}
