using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MedicalInventory.API.Models
{
    public class MedicalProduct
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public required string Name { get; set; }
        
        // SKU = Stock Keeping Unit (código interno del producto)
        [Required(ErrorMessage = "El SKU es obligatorio")]
        [StringLength(50)]
        public required string SKU { get; set; }
        
        [Required(ErrorMessage = "La categoría es obligatoria")]
        public MedicalProductCategory Category { get; set; }
        
        // Precio del producto en euros
        [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0.01 y 99999.99")]
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int MinimumStock { get; set; }
        
        // Código RFID único para identificar el producto - puede ser null si no tiene RFID
        [StringLength(100)]
        public string? RfidCode { get; set; }

        public DateTime ExpirationDate { get; set; }
        
        public DateTime ManufacturingDate { get; set; }
        
        // Número de lote (importante para trazabilidad) - puede ser null si no aplica
        [StringLength(50)]
        public string? BatchNumber { get; set; }
        
        // Indica si el producto requiere autorización especial para dispensarse
        public bool RequiresAuthorization { get; set; }
        
        // Indica si es un producto controlado (ej: medicamentos especiales)
        public bool IsControlled { get; set; }
        
        // Condiciones de almacenamiento (ej: "Refrigerado 2-8°C") - puede ser null si son condiciones estándar
        [StringLength(500)]
        public string? StorageConditions { get; set; }
        
        // Navegación: Lista de todos los movimientos de este producto
        // Esto nos permite ver el historial completo del producto
        public List<MedicalProductMovement> Movements { get; set; } = new List<MedicalProductMovement>();
        
        // Navegación: Lista de items en carritos que incluyen este producto
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        
        // Propiedades calculadas (solo lectura)
        
        // Indica si el producto está próximo a expirar (menos de 30 días)
        public bool IsNearExpiration => ExpirationDate <= DateTime.Now.AddDays(30);
        
        // Indica si el producto ya expiró
        public bool IsExpired => ExpirationDate < DateTime.Now;
        
        // Indica si el stock está por debajo del mínimo
        public bool IsLowStock => Stock <= MinimumStock;
        
        // Indica si el producto está disponible para dispensación
        public bool IsAvailable => Stock > 0 && !IsExpired && !IsControlled;
    }
    
}