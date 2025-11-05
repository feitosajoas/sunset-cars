using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunsetCars.Data.Repositories;
using SunsetCars.DTOs.Requests;
using SunsetCars.Models;
using SunsetCars.Services.Domain;
using SunsetCars.Services.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace SunsetCars.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
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

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            return View();
        }

        [HttpGet]
        [Route("api/customers")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllApi([FromQuery] string? searchString = null)
        {
            try
            {
                var result = await _customerService.GetAllCustomersAsync();

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao buscar clientes: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new { error = result.ErrorMessage });
                }

                var customers = result.Data ?? new List<DTOs.Responses.CustomerDto>();

                if (!string.IsNullOrEmpty(searchString))
                {
                    customers = customers.Where(c =>
                        c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        c.Cpf.Contains(searchString) ||
                        c.Phone.Contains(searchString)).ToList();
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clientes");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        [Route("api/customers/{id}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetByIdApi([FromRoute] int id)
        {
            try
            {
                var result = await _customerService.GetCustomerByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound(new { error = "Cliente não encontrado" });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        [Route("api/customers")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateApi([FromBody] CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _customerService.CreateCustomerAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao criar cliente: {ErrorMessage}", result.ErrorMessage);
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
                _logger.LogError(ex, "Erro ao criar cliente");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPut]
        [Route("api/customers/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateApi([FromRoute] int id, [FromBody] UpdateCustomerRequest request)
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
                var result = await _customerService.UpdateCustomerAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao atualizar cliente {Id}: {ErrorMessage}", id, result.ErrorMessage);
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
                _logger.LogError(ex, "Erro ao atualizar cliente {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpDelete]
        [Route("api/customers/{id}")]
        [Authorize(Roles = "Administrador")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteApi([FromRoute] int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Erro ao excluir cliente {Id}: {ErrorMessage}", id, result.ErrorMessage);
                    return BadRequest(new { error = result.ErrorMessage });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cliente {Id}", id);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        [Route("api/customers/cpf/{cpf}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> GetByCpfApi([FromRoute] [Required] string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return BadRequest(new { error = "CPF é obrigatório" });
            }

            try
            {
                var result = await _customerService.GetCustomerByCpfAsync(cpf);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound(new { error = "Cliente não encontrado" });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente por CPF {Cpf}", cpf);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        [Route("api/customers/search")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> SearchApi([FromQuery] [Required] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return BadRequest(new { error = "Termo de busca deve ter pelo menos 3 caracteres" });
            }

            try
            {
                var result = await _customerService.GetAllCustomersAsync();

                if (!result.IsSuccess || result.Data == null)
                {
                    return Ok(new List<object>());
                }

                var customers = result.Data
                    .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                               c.Cpf.Contains(term))
                    .Take(10)
                    .Select(c => new
                    {
                        id = c.Id,
                        label = $"{c.Name} - {c.Cpf}",
                        value = c.Name,
                        cpf = c.Cpf,
                        phone = c.Phone
                    })
                    .ToList();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clientes por termo {Term}", term);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }
    }
}
