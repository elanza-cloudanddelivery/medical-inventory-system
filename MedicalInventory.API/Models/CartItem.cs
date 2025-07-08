using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models
{

    public class CartItem
    {
        // Identificador único del item dentro del sistema
        public int Id { get; set; }
        
        // Referencia al carrito que contiene este item - siempre requerida
        [Required(ErrorMessage = "El carrito es obligatorio")]
        public int CartId { get; set; }
        
        // Navegación hacia el carrito padre que contiene este item
        // Puede ser null si no se carga explícitamente con Include()
        public Cart? Cart { get; set; }
        
        // Referencia al producto médico que se está solicitando - siempre requerida
        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductId { get; set; }
        
        // Navegación hacia el producto médico solicitado
        // Puede ser null si no se carga explícitamente con Include()
        public MedicalProduct? Product { get; set; }
        
        // Cantidad del producto que se está solicitando
        // Debe ser al menos 1 para que el item tenga sentido
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0")]
        public int Quantity { get; set; }
        
        // Precio unitario del producto al momento de agregar al carrito
        // Se captura en este momento porque los precios pueden cambiar mientras el carrito está activo
        // Importante para valoración consistente del inventario
        public decimal UnitPrice { get; set; }
        
        // Fecha y hora exacta cuando se agregó este item al carrito
        // Útil para auditorías y análisis de patrones de uso
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        // Notas específicas para este item individual
        // Ejemplos: "Para sala de operaciones 3", "Uso en procedimiento de emergencia"
        [StringLength(500)]
        public string? ItemNotes { get; set; }
        
        // === PROPIEDADES CALCULADAS PARA VALIDACIONES Y LÓGICA DE NEGOCIO ===
        
        // Calcula el precio total de este item (cantidad × precio unitario)
        // Esta propiedad asegura que los cálculos de precio sean consistentes
        public decimal TotalPrice => Quantity * UnitPrice;
        
        // Verifica si hay stock suficiente del producto para satisfacer esta solicitud
        // Crítico para prevenir dispensaciones imposibles de completar
        public bool HasSufficientStock => Product?.Stock >= Quantity;
        
        // Verifica si el producto asociado está expirado
        // Fundamental para la seguridad del paciente
        public bool IsProductExpired => Product?.IsExpired == true;
        
        // Indica si el producto requiere autorización especial para ser dispensado
        // Importante para implementar flujos de aprobación adicionales
        public bool RequiresAuthorization => Product?.RequiresAuthorization == true;
        
        // Indica si el producto es de tipo controlado (medicamentos especiales, etc.)
        // Requiere validaciones adicionales y registro especial
        public bool IsControlledProduct => Product?.IsControlled == true;
        
        // Calcula cuánto tiempo ha estado este item en el carrito
        // Útil para identificar items que han estado esperando mucho tiempo
        public TimeSpan TimeInCart => DateTime.Now - AddedAt;
        
        // Verifica si este item está cerca de expirar mientras está en el carrito
        // Importante para carritos que permanecen activos por períodos prolongados
        public bool IsNearExpiration => Product?.IsNearExpiration == true;
        
        // Proporciona una descripción del estado del item para interfaces de usuario
        // Combina múltiples validaciones en un mensaje legible
        public string ItemStatus
        {
            get
            {
                if (IsProductExpired) return "Producto expirado";
                if (!HasSufficientStock) return "Stock insuficiente";
                if (IsNearExpiration) return "Próximo a expirar";
                if (IsControlledProduct) return "Producto controlado";
                if (RequiresAuthorization) return "Requiere autorización";
                return "Disponible";
            }
        }
        
        // Indica si este item puede ser dispensado de manera segura
        // Combina todas las validaciones críticas para la seguridad médica
        public bool CanBeDispensed => HasSufficientStock && 
                                     !IsProductExpired && 
                                     Product?.IsAvailable == true;
        
        // Calcula el valor del stock que se está solicitando como porcentaje del total disponible
        // Útil para identificar solicitudes que agotan significativamente el inventario
        public decimal StockUsagePercentage => Product?.Stock > 0 
            ? (decimal)Quantity / Product.Stock * 100 
            : 0;
        
        // Indica si esta solicitud agotaría completamente el stock del producto
        // Importante para alertas de reabastecimiento crítico
        public bool WouldDepletStock => Product?.Stock == Quantity;
        
        // Indica si esta solicitud dejaría el stock por debajo del mínimo establecido
        // Trigger para procesos automáticos de reposición
        public bool WouldTriggerReorder => Product != null && 
                                          (Product.Stock - Quantity) <= Product.MinimumStock;
    }
}