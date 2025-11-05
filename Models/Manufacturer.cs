using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SunsetCars.Models
{
    [Index(nameof(Name), IsUnique = true, Name = "IX_Manufacturer_Name")]
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Fabricante")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O país de origem é obrigatório")]
        [StringLength(50, ErrorMessage = "O país deve ter no máximo 50 caracteres")]
        [Display(Name = "País de Origem")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano de fundação é obrigatório")]
        [Range(1800, 2024, ErrorMessage = "Ano deve estar entre 1800 e o ano atual")]
        [Display(Name = "Ano de Fundação")]
        public int FoundedYear { get; set; }

        [StringLength(255, ErrorMessage = "O website deve ter no máximo 255 caracteres")]
        [Url(ErrorMessage = "Formato de URL inválido")]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}