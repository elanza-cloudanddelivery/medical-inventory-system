using MedicalInventory.API.Data;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Services.Common;

namespace MedicalInventory.API.Services.Validators.CartValidator;

public class CartValidator : ICartValidator
{
    private readonly MedicalInventoryDbContext _context;
    private readonly ILogger<CartValidator> _logger;

    public CartValidator(MedicalInventoryDbContext context, ILogger<CartValidator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAddItemAsync(int userId, AddToCartRequest request)
    {
    
        var product = await _context.MedicalProducts.FindAsync(request.ProductId);
        if (product == null)
        {
            return ValidationResult.ErrorResult("Producto no encontrado");
        }

        var productValidation = ValidateProduct(product, request.Quantity);
        if (!productValidation.Success)
        {
            return productValidation;
        }

        return await ValidateUserPermissionsAsync(product, userId);
    }

    public ValidationResult ValidateProduct(MedicalProduct product, int requestedQuantity)
    {
        if (product.IsExpired)
        {
            return ValidationResult.ErrorResult($"El producto '{product.Name}' ha expirado el {product.ExpirationDate:dd/MM/yyyy}");
        }

        if (product.Stock < requestedQuantity)
        {
            return ValidationResult.ErrorResult($"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}, Solicitado: {requestedQuantity}");
        }

        // 3. Advertencias (no bloquean)
        if (product.IsNearExpiration)
        {
            _logger.LogWarning("Producto próximo a expirar - Producto: {ProductName}, Expira: {ExpirationDate}",
                product.Name, product.ExpirationDate);
        }

        if (product.Stock - requestedQuantity <= product.MinimumStock)
        {
            _logger.LogWarning("Stock quedará bajo - Producto: {ProductName}, Stock resultante: {ResultingStock}, Mínimo: {MinimumStock}",
                product.Name, product.Stock - requestedQuantity, product.MinimumStock);
        }

        return ValidationResult.SuccessResult();
    }

    public async Task<ValidationResult> ValidateUserPermissionsAsync(MedicalProduct product, int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ValidationResult.ErrorResult("Usuario no encontrado");
            }

            if (!user.IsActive)
            {
                return ValidationResult.ErrorResult("Usuario inactivo. Contacte al administrador.");
            }

            // Productos controlados
            if (product.IsControlled)
            {
                if (user.Role != UserRole.Doctor &&
                    user.Role != UserRole.Pharmacist &&
                    user.Role != UserRole.SuperAdministrator)
                {
                    _logger.LogWarning("Usuario sin permisos para producto controlado - Usuario: {Username} (Role: {Role}), Producto: {ProductName}",
                        user.FullName, user.Role, product.Name);

                    return ValidationResult.ErrorResult($"No tiene permisos para '{product.Name}' (producto controlado). Solo médicos, farmacéuticos y super administradores.");
                }
            }

            // Productos que requieren autorización
            if (product.RequiresAuthorization)
            {
                if (user.Role == UserRole.Technician || user.Role == UserRole.Viewer)
                {
                    _logger.LogWarning("Usuario sin permisos para producto con autorización - Usuario: {Username} (Role: {Role}), Producto: {ProductName}",
                        user.FullName, user.Role, product.Name);

                    return ValidationResult.ErrorResult($"No tiene permisos para '{product.Name}' que requiere autorización especial.");
                }
            }

            return ValidationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando permisos de usuario - UserId: {UserId}, ProductId: {ProductId}",
                userId, product.Id);

            return ValidationResult.ErrorResult("Error verificando permisos");
        }
    }
}
