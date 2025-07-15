using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;

public class MedicalProductFilterRequest
{
 
    [StringLength(100, ErrorMessage = "El término de búsqueda no puede exceder 100 caracteres")]
    public string? SearchTerm { get; set; }

}