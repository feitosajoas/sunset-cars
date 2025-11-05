using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SunsetCars.Models
{
    [Index(nameof(Name), IsUnique = true, Name = "IX_Dealership_Name")]
    public class Dealership
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da concessionária é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome da Concessionária")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço é obrigatório")]
        [StringLength(255, ErrorMessage = "O endereço deve ter no máximo 255 caracteres")]
        [Display(Name = "Endereço")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [StringLength(50, ErrorMessage = "A cidade deve ter no máximo 50 caracteres")]
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório")]
        [StringLength(50, ErrorMessage = "O estado deve ter no máximo 50 caracteres")]
        [Display(Name = "Estado")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A capacidade máxima é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser maior que zero")]
        [Display(Name = "Capacidade Máxima de Veículos")]
        public int MaxCapacity { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}