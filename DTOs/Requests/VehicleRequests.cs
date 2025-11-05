using System.ComponentModel.DataAnnotations;
using SunsetCars.Models;

namespace SunsetCars.DTOs.Requests;

public class CreateVehicleRequest
{
    [Required(ErrorMessage = "Selecione um fabricante")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um fabricante válido")]
    [Display(Name = "Fabricante")]
    public int ManufacturerId { get; set; }

    [Required(ErrorMessage = "O modelo é obrigatório")]
    [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres")]
    [Display(Name = "Modelo")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ano é obrigatório")]
    [Range(1900, 2100, ErrorMessage = "Ano deve estar entre 1900 e 2100")]
    [Display(Name = "Ano")]
    public int Year { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    [DataType(DataType.Currency)]
    [Display(Name = "Preço")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Selecione o tipo de veículo")]
    [Display(Name = "Tipo de Veículo")]
    public VehicleType Type { get; set; }

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
}

public class UpdateVehicleRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione um fabricante")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um fabricante válido")]
    [Display(Name = "Fabricante")]
    public int ManufacturerId { get; set; }

    [Required(ErrorMessage = "O modelo é obrigatório")]
    [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres")]
    [Display(Name = "Modelo")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ano é obrigatório")]
    [Range(1900, 2100, ErrorMessage = "Ano deve estar entre 1900 e 2100")]
    [Display(Name = "Ano")]
    public int Year { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    [DataType(DataType.Currency)]
    [Display(Name = "Preço")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Selecione o tipo de veículo")]
    [Display(Name = "Tipo de Veículo")]
    public VehicleType Type { get; set; }

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
}
