using MedicalInventory.API.Models.DTOs.Common;

namespace MedicalInventory.API.Models.DTOs.Response;

public class MedicalProductResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<MedicalProductDto> Products { get; set; } = new();

}

