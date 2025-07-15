using MedicalInventory.API.Models.DTOs.Common;

namespace MedicalInventory.API.Models.DTOs.Response;

public class CartResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CartDto? Cart { get; set; }
}