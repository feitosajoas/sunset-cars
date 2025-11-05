using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SunsetCars.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A concessionária é obrigatória")]
        [Display(Name = "Concessionária")]
        public int DealershipId { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório")]
        [Display(Name = "Veículo")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório")]
        [Display(Name = "Cliente")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "A data da venda é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Venda")]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "O preço de venda é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preço de Venda")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal SalePrice { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Protocolo")]
        public string Protocol { get; set; } = string.Empty;

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Vendedor")]
        public string? SalesPersonId { get; set; }

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        [ForeignKey("DealershipId")]
        public virtual Dealership? Dealership { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("SalesPersonId")]
        public virtual ApplicationUser? SalesPerson { get; set; }
    }
}