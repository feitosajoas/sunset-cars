namespace SunsetCars.DTOs.Responses;

public class SaleDto
{
    public int Id { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal SalePrice { get; set; }
    public int DealershipId { get; set; }
    public string DealershipName { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleModel { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCpf { get; set; } = string.Empty;
    public string SalesPersonName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class SaleDetailsDto : SaleDto
{
    public DealershipDto Dealership { get; set; } = null!;
    public VehicleDto Vehicle { get; set; } = null!;
    public CustomerDto Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class VehicleDto
{
    public int Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VehicleDetailsDto : VehicleDto
{
    public ManufacturerDto Manufacturer { get; set; } = null!;
}

public class ManufacturerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int FoundedYear { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
}

public class DealershipDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
