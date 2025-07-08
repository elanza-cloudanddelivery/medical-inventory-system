using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalInventory.API.Models
{

    public class MedicalProductMovement
    {

        public int Id { get; set; }
        
        // SIEMPRE debe estar asociado a un producto
        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductId { get; set; }
        
        // Navegación hacia el producto relacionado
        // Esta propiedad nos permite acceder a toda la información del producto
        // sin hacer consultas adicionales a la base de datos
        public MedicalProduct? Product { get; set; }
        
        // SIEMPRE debe estar asociado a un usuario
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public int UserId { get; set; }
        
        // Navegación hacia el usuario que realizó el movimiento
        public User? User { get; set; }
        
        // Tipo de movimiento (entrada, salida, ajuste, etc.)
        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        public MedicalProductMovementType Type { get; set; }
        public int Quantity { get; set; }
        
        // Fecha y hora exacta del movimiento
        // Crucial para auditorías y trazabilidad
        public DateTime Timestamp { get; set; }
        
        // Razón del movimiento (ej: "Cirugía cardíaca", "Reposición de stock")
        // Opcional pero muy útil para auditorías - puede ser null si no se especifica
        [StringLength(500, ErrorMessage = "La razón no puede exceder 500 caracteres")]
        public string? Reason { get; set; }
        
        // Departamento donde se realizó el movimiento
        // Opcional pero importante para estadísticas y control - puede ser null
        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public string? Department { get; set; }
        
        // Referencia al número de lote específico (si aplica)
        // Útil cuando un producto tiene múltiples lotes - puede ser null si no aplica
        [StringLength(50)]
        public string? BatchNumber { get; set; }
        
        // Costo unitario del producto al momento del movimiento
        // Importante para valoración de inventario y estadísticas financieras
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }
        
        // Campo libre para información adicional - puede ser null si no hay notas
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        // Indica si el movimiento fue parte de una transacción automática
        // Útil para diferenciar entre movimientos manuales y automáticos
        public bool IsAutomated { get; set; }
        
        // Propiedades calculadas para facilitar la consulta de información
        
        // Calcula el valor total del movimiento (cantidad × costo unitario)
        public decimal TotalValue => Math.Abs(Quantity) * UnitCost;
        
        // Indica si el movimiento reduce el stock (salidas, daños, etc.)
        public bool IsStockReduction => Type == MedicalProductMovementType.StockOut || 
                                       Type == MedicalProductMovementType.Expired || 
                                       Type == MedicalProductMovementType.Damaged;
        
        // Indica si el movimiento aumenta el stock (entradas, devoluciones)
        public bool IsStockIncrease => Type == MedicalProductMovementType.StockIn || 
                                      Type == MedicalProductMovementType.Return;
        
        // Indica si el movimiento es crítico (requiere mayor trazabilidad)
        public bool IsCriticalMovement => Type == MedicalProductMovementType.StockOut || 
                                         Type == MedicalProductMovementType.Expired || 
                                         Type == MedicalProductMovementType.Damaged;
        
        // Descripción legible del tipo de movimiento
        public string MovementDescription => Type switch
        {
            MedicalProductMovementType.StockIn => "Entrada de stock",
            MedicalProductMovementType.StockOut => "Dispensación",
            MedicalProductMovementType.Adjustment => "Ajuste de inventario",
            MedicalProductMovementType.Transfer => "Transferencia",
            MedicalProductMovementType.Expired => "Producto expirado",
            MedicalProductMovementType.Damaged => "Producto dañado",
            MedicalProductMovementType.Return => "Devolución",
            _ => "Movimiento desconocido"
        };
    }
    

}