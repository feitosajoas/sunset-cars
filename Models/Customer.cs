using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SunsetCars.Models
{
    [Index(nameof(CPF), IsUnique = true, Name = "IX_Customer_CPF")]
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres")]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
        [Display(Name = "CPF")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Data de Cadastro")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
