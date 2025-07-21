using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models.DTOs.Request;

public class UpdateItemCartRequest
{
    [Required(ErrorMessage = "La nueva cantidad es obligatoria")]
    [Range(0, 100, ErrorMessage = "La cantidad debe estar entre 0 y 100")]
    public int NewQuantity { get; set; }
}