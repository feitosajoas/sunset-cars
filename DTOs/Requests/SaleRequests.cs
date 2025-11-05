using System.ComponentModel.DataAnnotations;

namespace SunsetCars.DTOs.Requests;

public class CreateSaleRequest
{
    [Required(ErrorMessage = "Selecione uma concessionária")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma concessionária válida")]
    public int DealershipId { get; set; }

    [Required(ErrorMessage = "Selecione um veículo")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um veículo válido")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Selecione um cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente válido")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Informe o preço de venda")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    [DataType(DataType.Currency)]
    [Display(Name = "Preço de Venda")]
    public decimal SalePrice { get; set; }
}

public class UpdateSaleRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione uma concessionária")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma concessionária válida")]
    public int DealershipId { get; set; }

    [Required(ErrorMessage = "Selecione um veículo")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um veículo válido")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Selecione um cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente válido")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Informe o preço de venda")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    [DataType(DataType.Currency)]
    [Display(Name = "Preço de Venda")]
    public decimal SalePrice { get; set; }

    [Required(ErrorMessage = "Informe a data da venda")]
    [DataType(DataType.Date)]
    [Display(Name = "Data da Venda")]
    public DateTime SaleDate { get; set; }

    public string Protocol { get; set; } = string.Empty;
}
