using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;

namespace MedicalInventory.API.Services.CartService;

public interface ICartService
{
    Task<CartResponse> AddItemToCartAsync(int userId, AddToCartRequest request);
    Task<CartDto?> GetActiveCartAsync(int userId);
    Task<CartResponse> UpdateCartItemAsync(int userId, int itemId, int newQuantity);
    Task<CartResponse> RemoveItemFromCartAsync(int userId, int itemId);
    Task<CartResponse> ClearCartAsync(int userId);
}