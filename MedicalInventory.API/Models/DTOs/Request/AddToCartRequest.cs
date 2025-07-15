using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;

public class AddToCartRequest
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a 0")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
    public int Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Las notas del item no pueden exceder 500 caracteres")]
    public string? ItemNotes { get; set; }

    [StringLength(500, ErrorMessage = "El propósito no puede exceder 500 caracteres")]
    public string? Purpose { get; set; }

    [StringLength(100, ErrorMessage = "El departamento objetivo no puede exceder 100 caracteres")]
    public string? TargetDepartment { get; set; }

    public DispensationPriority Priority { get; set; } = DispensationPriority.Normal;
}