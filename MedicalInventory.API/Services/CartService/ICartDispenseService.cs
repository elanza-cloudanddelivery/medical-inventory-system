using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;

namespace MedicalInventory.API.Services.CartService;
public interface ICartDispenseService
{
    Task<CartResponse> DispenseCartAsync(int userId, DispenseCartRequest request);
    Task<bool> CanUserDispenseCartAsync(int userId);
}