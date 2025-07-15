using System.ComponentModel.DataAnnotations;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Request;
using ValidationResult = MedicalInventory.API.Services.Common.ValidationResult;

namespace MedicalInventory.API.Services.Validators.CartValidator;

public interface ICartValidator
{
    Task<ValidationResult> ValidateAddItemAsync(int userId, AddToCartRequest request);
    ValidationResult ValidateProduct(MedicalProduct product, int requestedQuantity);
    Task<ValidationResult> ValidateUserPermissionsAsync(MedicalProduct product, int userId);
}