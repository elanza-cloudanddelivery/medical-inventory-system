using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MedicalInventory.API.Models
{
    // Carrito de dispensación médica para agrupar productos antes de confirmar
    public class Cart
    {
        // === IDENTIFICACIÓN Y ESTADO BÁSICO ===
        
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public int UserId { get; set; }
        
        // Navegación - puede ser null si no se carga con Include()
        public User? User { get; set; }
        
        // === FECHAS Y TIEMPOS ===
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public DateTime? ConfirmedAt { get; set; } // Null hasta confirmación
        
        // === CONFIGURACIÓN ===
        
        [Required(ErrorMessage = "El estado del carrito es obligatorio")]
        public CartStatus Status { get; set; } = CartStatus.Active;
        
        public DispensationPriority Priority { get; set; } = DispensationPriority.Normal;
        
        // Campos opcionales para contexto médico
        [StringLength(500)]
        public string? Purpose { get; set; } // Ej: "Cirugía cardíaca"
        
        [StringLength(100)]
        public string? TargetDepartment { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        // === RELACIÓN CON ITEMS ===
        
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        // === MÉTRICAS BÁSICAS ===
        
        public int TotalItems => Items?.Count ?? 0;
        public int TotalQuantity => Items?.Sum(i => i.Quantity) ?? 0;
        public decimal TotalValue => Items?.Sum(i => i.TotalPrice) ?? 0;
        public bool IsEmpty => TotalItems == 0;
        
        // === ESTADOS DERIVADOS ===
        
        public bool IsActive => Status == CartStatus.Active;
        public bool IsConfirmed => Status == CartStatus.Confirmed;
        public TimeSpan ActiveTime => DateTime.Now - CreatedAt;
        public bool IsStale => ActiveTime.TotalHours > 2 && Status == CartStatus.Active;
        
        // === COLECCIONES DE ITEMS ESPECÍFICOS ===
        // Cada colección filtra items por un tipo específico de problema o característica
        
        public IEnumerable<CartItem> ControlledProductItems => 
            Items?.Where(item => item.IsControlledProduct) ?? Enumerable.Empty<CartItem>();
        
        public IEnumerable<CartItem> ExpiredProductItems => 
            Items?.Where(item => item.IsProductExpired) ?? Enumerable.Empty<CartItem>();
        
        public IEnumerable<CartItem> InsufficientStockItems => 
            Items?.Where(item => !item.HasSufficientStock) ?? Enumerable.Empty<CartItem>();
        
        public IEnumerable<CartItem> NearExpirationItems => 
            Items?.Where(item => item.IsNearExpiration) ?? Enumerable.Empty<CartItem>();
        
        public IEnumerable<CartItem> AuthorizationRequiredItems => 
            Items?.Where(item => item.RequiresAuthorization) ?? Enumerable.Empty<CartItem>();
        
        // Colección master de todos los items con problemas
        public IEnumerable<CartItem> ProblematicItems => 
            Items?.Where(item => !item.CanBeDispensed) ?? Enumerable.Empty<CartItem>();
        
        // === VALIDACIONES BOOLEANAS ===
        // Derivadas de las colecciones anteriores para evitar recálculos
        
        public bool HasControlledProducts => ControlledProductItems.Any();
        public bool HasExpiredProducts => ExpiredProductItems.Any();
        public bool HasInsufficientStock => InsufficientStockItems.Any();
        public bool HasNearExpirationProducts => NearExpirationItems.Any();
        public bool RequiresSpecialAuthorization => AuthorizationRequiredItems.Any();
        
        // === CONTADORES ===
        
        public int ProblematicItemsCount => ProblematicItems.Count();
        public int ControlledProductsCount => ControlledProductItems.Count();
        public int ExpiredProductsCount => ExpiredProductItems.Count();
        public int InsufficientStockCount => InsufficientStockItems.Count();
        
        // === VALIDACIONES PRINCIPALES ===
        
        public bool AllItemsReady => !IsEmpty && ProblematicItemsCount == 0;
        public bool CanBeConfirmed => IsActive && AllItemsReady;
        
        // === DESCRIPCIONES PARA UI ===
        
        public string StatusDescription => Status switch
        {
            CartStatus.Active => "Activo",
            CartStatus.Confirmed => "Confirmado", 
            CartStatus.Cancelled => "Cancelado",
            CartStatus.Expired => "Expirado",
            _ => "Estado Desconocido"
        };
        
        public string PriorityDescription => Priority switch
        {
            DispensationPriority.Normal => "Normal",
            DispensationPriority.Urgent => "Urgente",
            DispensationPriority.Emergency => "Emergencia",
            _ => "Prioridad Desconocida"
        };
        
        // Resumen dinámico del estado del carrito
        public string StatusSummary
        {
            get
            {
                if (IsEmpty) return "Carrito vacío";
                if (!IsActive) return $"Carrito {StatusDescription.ToLower()}";
                if (AllItemsReady) return "Todos los items listos para dispensación";
                
                var issues = new List<string>();
                if (ExpiredProductsCount > 0) 
                    issues.Add($"{ExpiredProductsCount} producto(s) expirado(s)");
                if (InsufficientStockCount > 0) 
                    issues.Add($"{InsufficientStockCount} producto(s) sin stock suficiente");
                if (ControlledProductsCount > 0) 
                    issues.Add($"{ControlledProductsCount} producto(s) controlado(s)");
                
                return issues.Any() ? $"Requiere atención: {string.Join(", ", issues)}" : "Items listos";
            }
        }
    }
}