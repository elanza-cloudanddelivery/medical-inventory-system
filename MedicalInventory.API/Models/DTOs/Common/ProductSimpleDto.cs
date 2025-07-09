namespace MedicalInventory.API.Models.DTOs.Common;

public class ProductSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public int CategoryCode { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? BatchNumber { get; set; }
        public string? StorageConditions { get; set; }
        
        // Estados calculados
        public bool IsExpired { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsNearExpiration { get; set; }
        public int DaysUntilExpiration { get; set; }
        public bool IsAvailable { get; set; }
        
        // Información de control
        public bool IsControlled { get; set; }
        public bool RequiresAuthorization { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
    }
