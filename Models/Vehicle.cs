using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SunsetCars.Models
{
    public enum VehicleType
    {
        [Display(Name = "Carro")]
        Car = 1,
        [Display(Name = "Moto")]
        Motorcycle = 2,
        [Display(Name = "Caminhão")]
        Truck = 3,
        [Display(Name = "SUV")]
        SUV = 4,
        [Display(Name = "Pickup")]
        Pickup = 5,
        [Display(Name = "Van")]
        Van = 6
    }

    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres")]
        [Display(Name = "Modelo")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano de fabricação é obrigatório")]
        [Range(1900, 2030, ErrorMessage = "Ano deve estar entre 1900 e 2030")]
        [Display(Name = "Ano de Fabricação")]
        public int Year { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preço")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "O fabricante é obrigatório")]
        [Display(Name = "Fabricante")]
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "O tipo de veículo é obrigatório")]
        [Display(Name = "Tipo de Veículo")]
        public VehicleType Type { get; set; }

        [Column(TypeName = "TEXT")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Relacionamentos
        [ForeignKey("ManufacturerId")]
        public virtual Manufacturer? Manufacturer { get; set; }

        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}