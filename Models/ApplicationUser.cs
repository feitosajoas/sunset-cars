using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SunsetCars.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}