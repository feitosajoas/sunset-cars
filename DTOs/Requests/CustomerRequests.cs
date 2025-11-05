using System.ComponentModel.DataAnnotations;

namespace SunsetCars.DTOs.Requests;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    [Display(Name = "Nome Completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório")]
    [RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
    [Display(Name = "CPF")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [Phone(ErrorMessage = "Formato de telefone inválido")]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = string.Empty;
}

public class UpdateCustomerRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    [Display(Name = "Nome Completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório")]
    [RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
    [Display(Name = "CPF")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [Phone(ErrorMessage = "Formato de telefone inválido")]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = string.Empty;
}
