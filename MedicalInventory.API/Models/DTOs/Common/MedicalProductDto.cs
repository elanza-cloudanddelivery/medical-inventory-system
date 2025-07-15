namespace MedicalInventory.API.Models.DTOs.Common;

public class MedicalProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CategoryCode { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public string? RfidCode { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime ManufacturingDate { get; set; }
    public string? BatchNumber { get; set; }
    public bool RequiresAuthorization { get; set; }
    public bool IsControlled { get; set; }
    public string? StorageConditions { get; set; }

    // Estados calculados
    public bool IsNearExpiration { get; set; }
    public bool IsExpired { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsAvailable { get; set; }
    public bool CanBeAddedToCart { get; set; }

}