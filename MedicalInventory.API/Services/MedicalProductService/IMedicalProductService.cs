using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;

namespace MedicalInventory.API.Services.MedicalProductService;

public interface IMedicalProductService
    {
        Task<MedicalProductResponse> GetAvailableProductsAsync(int userId, string userRole);
        Task<MedicalProductResponse> SearchProductsAsync(MedicalProductFilterRequest filter, int userId, string userRole);
        Task<MedicalProductDto?> GetProductByIdAsync(int productId, int userId, string userRole);
        Task<List<MedicalProductDto>> GetProductsByCategoryAsync(int categoryId, int userId, string userRole);
    }