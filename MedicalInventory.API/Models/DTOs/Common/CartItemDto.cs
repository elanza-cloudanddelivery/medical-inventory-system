namespace MedicalInventory.API.Models.DTOs.Common;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    // Información del producto
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;

    // Cantidad y precios
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Fechas y notas
    public DateTime AddedAt { get; set; }
    public string? ItemNotes { get; set; }

    // Estados y validaciones
    public string ItemStatus { get; set; } = string.Empty;
    public bool CanBeDispensed { get; set; }
    public bool HasSufficientStock { get; set; }
    public bool IsProductExpired { get; set; }
    public bool IsControlledProduct { get; set; }
    public bool RequiresAuthorization { get; set; }
    public bool IsNearExpiration { get; set; }
    public bool WouldTriggerReorder { get; set; }

    // Información adicional del producto
    public int AvailableStock { get; set; }
    public int MinimumStock { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
}