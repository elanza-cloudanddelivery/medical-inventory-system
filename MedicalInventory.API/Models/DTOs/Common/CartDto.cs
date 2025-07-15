namespace MedicalInventory.API.Models.DTOs.Common;

public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Estado del carrito
    public string Status { get; set; } = string.Empty;
    public string StatusSummary { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    // Información adicional
    public string? Purpose { get; set; }
    public string? TargetDepartment { get; set; }
    public string? Notes { get; set; }

    // Fechas
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Métricas del carrito
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public bool IsEmpty { get; set; }
    public bool CanBeConfirmed { get; set; }
    public bool HasProblems { get; set; }

    // Contadores de problemas
    public int ProblematicItemsCount { get; set; }
    public int ControlledProductsCount { get; set; }
    public int ExpiredProductsCount { get; set; }
    public int InsufficientStockCount { get; set; }

    // Items del carrito
    public List<CartItemDto> Items { get; set; } = new();
}