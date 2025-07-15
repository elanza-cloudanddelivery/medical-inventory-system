using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;

public class DispenseCartRequest
{
    [StringLength(500, ErrorMessage = "La razón no puede exceder 500 caracteres")]
    public string? Reason { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }

    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; set; }
}