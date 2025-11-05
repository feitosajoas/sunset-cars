using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Requests;
using SunsetCars.Models;
using SunsetCars.Services;
using SunsetCars.Services.Domain;
using SunsetCars.Services.Infrastructure;

namespace SunsetCars.Controllers
{
    [Authorize]
    public class DealershipsController : Controller
    {
        private readonly IDealershipService _dealershipService;
        private readonly ICepService _cepService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DealershipsController> _logger;

        public DealershipsController(
            IDealershipService dealershipService,
            ICepService cepService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<DealershipsController> logger)
        {
            _dealershipService = dealershipService;
            _cepService = cepService;
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

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            return View();
        }

        [HttpGet]
        [Route("api/dealerships")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllApi([FromQuery] string? searchString = null)
        {
            try
            {
                var result = await _dealershipService.GetActiveDealershipsAsync();

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao buscar concessionárias: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new { error = result.ErrorMessage });
                }

                var dealerships = result.Data ?? new List<DTOs.Responses.DealershipDto>();

                if (!string.IsNullOrEmpty(searchString))
                {
                    dealerships = dealerships.Where(d =>
                        d.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        d.City.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        d.State.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(dealerships);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar concessionárias");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        [Route("api/dealerships/{id}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetByIdApi([FromRoute] int id)
        {
            try
            {
                var result = await _dealershipService.GetDealershipByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound(new { error = "Concessionária não encontrada" });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar concessionária {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        [Route("api/dealerships")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateApi([FromBody] CreateDealershipRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _dealershipService.CreateDealershipAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao criar concessionária: {ErrorMessage}", result.ErrorMessage);
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
                _logger.LogError(ex, "Erro ao criar concessionária");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPut]
        [Route("api/dealerships/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateApi([FromRoute] int id, [FromBody] UpdateDealershipRequest request)
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
                var result = await _dealershipService.UpdateDealershipAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao atualizar concessionária {Id}: {ErrorMessage}", id, result.ErrorMessage);
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
                _logger.LogError(ex, "Erro ao atualizar concessionária {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpDelete]
        [Route("api/dealerships/{id}")]
        [Authorize(Roles = "Administrador")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteApi([FromRoute] int id)
        {
            try
            {
                var result = await _dealershipService.DeleteDealershipAsync(id);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao excluir concessionária {Id}: {ErrorMessage}", id, result.ErrorMessage);
                    return BadRequest(new { error = result.ErrorMessage });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir concessionária {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        [Route("api/dealerships/cep/{cep}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetAddressByCepApi([FromRoute] string cep)
        {
            if (string.IsNullOrEmpty(cep))
            {
                return BadRequest(new { error = "CEP é obrigatório" });
            }

            try
            {
                var address = await _cepService.GetAddressByCepAsync(cep);

                if (address == null)
                {
                    return NotFound(new { error = "CEP não encontrado" });
                }

                return Ok(new
                {
                    street = address.Logradouro,
                    city = address.Localidade,
                    state = address.Uf
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar endereço por CEP {Cep}", cep);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }
    }
}