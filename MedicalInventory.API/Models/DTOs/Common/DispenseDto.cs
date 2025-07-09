namespace MedicalInventory.API.Models.DTOs.Common;

public class DispenseDto
    {
        public int MovementId { get; set; }
        public ProductSimpleDto Product { get; set; } = new();
        public int QuantityDispensed { get; set; }
        public int RemainingStock { get; set; }
        public string DispensedBy { get; set; } = string.Empty;
        public DateTime DispensedAt { get; set; }
        public string? Reason { get; set; }
        public string? Department { get; set; }
        public string? BatchNumber { get; set; }
        public string? Notes { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public string MovementType { get; set; } = "Dispensación";
    }