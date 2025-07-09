using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;

 public class DispenseRequest
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a 0")]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
        public int Quantity { get; set; }
        
        [StringLength(500, ErrorMessage = "La razón no puede exceder 500 caracteres")]
        public string? Reason { get; set; }
        
        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public string? Department { get; set; }
        
        [StringLength(50, ErrorMessage = "El número de lote no puede exceder 50 caracteres")]
        public string? BatchNumber { get; set; }
        
        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
        public string? Notes { get; set; }
    }
