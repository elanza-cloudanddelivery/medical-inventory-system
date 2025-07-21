using MedicalInventory.API.Data;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Services.Validators.CartValidator;
using Microsoft.EntityFrameworkCore;

namespace MedicalInventory.API.Services.CartService;
public class CartDispenseService : ICartDispenseService
{
    private readonly MedicalInventoryDbContext _context;
    private readonly ICartValidator _validator;
    private readonly ILogger<CartDispenseService> _logger;

    public CartDispenseService(
        MedicalInventoryDbContext context,
        ICartValidator validator,
        ILogger<CartDispenseService> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CartResponse> DispenseCartAsync(int userId, DispenseCartRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null || cart.IsEmpty)
            {
                return new CartResponse
                {
                    Success = false,
                    Message = "No hay carrito activo o está vacío"
                };
            }

            if (!cart.CanBeConfirmed)
            {
                _logger.LogWarning("Intento de dispensar carrito con problemas - Usuario: {UserId}, Problemas: {StatusSummary}",
                    userId, cart.StatusSummary);

                return new CartResponse
                {
                    Success = false,
                    Message = $"No se puede dispensar el carrito: {cart.StatusSummary}"
                };
            }

            var user = await _context.Users.FindAsync(userId);
            var userName = user?.FullName ?? "Usuario desconocido";
            var userDepartment = user?.Department;

            foreach (var item in cart.Items.Where(i => i.CanBeDispensed))
            {
                var realtimeValidation = _validator.ValidateProduct(item.Product!, item.Quantity);
                if (!realtimeValidation.Success)
                {
                    await transaction.RollbackAsync();
                    return new CartResponse
                    {
                        Success = false,
                        Message = $"Error en {item.Product?.Name}: {realtimeValidation.Message}"
                    };
                }

                item.Product!.Stock -= item.Quantity;

                var movement = new MedicalProductMovement
                {
                    ProductId = item.ProductId,
                    UserId = userId,
                    Type = MedicalProductMovementType.StockOut,
                    Quantity = -item.Quantity,
                    Timestamp = DateTime.UtcNow,
                    Reason = request.Reason ?? $"Dispensación desde carrito #{cart.Id}",
                    Department = request.Department ?? userDepartment,
                    BatchNumber = item.Product.BatchNumber,
                    UnitCost = item.UnitPrice,
                    Notes = $"Carrito #{cart.Id}. {item.ItemNotes}. {request.Notes}".Trim(),
                    IsAutomated = false
                };

                _context.MedicalProductMovements.Add(movement);

                _logger.LogInformation("Producto dispensado - Usuario: {UserName}, Producto: {ProductName}, Cantidad: {Quantity}, Stock restante: {RemainingStock}",
                    userName, item.Product.Name, item.Quantity, item.Product.Stock);

                if (item.Product.IsLowStock)
                {
                    _logger.LogWarning("ALERTA: Stock bajo después de dispensación - Producto: {ProductName}, Stock: {Stock}, Mínimo: {MinStock}",
                        item.Product.Name, item.Product.Stock, item.Product.MinimumStock);
                }
            }

            cart.Status = CartStatus.Confirmed;
            cart.ConfirmedAt = DateTime.UtcNow;
            cart.LastModifiedAt = DateTime.UtcNow;
            cart.TargetDepartment = request.Department;
            cart.Notes = request.Notes;
            cart.Purpose = request.Reason;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Carrito dispensado exitosamente - Usuario: {UserName} (ID: {UserId}), CartId: {CartId}, Items: {ItemCount}, Valor: {TotalValue}",
                userName, userId, cart.Id, cart.TotalItems, cart.TotalValue);

            return new CartResponse
            {
                Success = true,
                Message = $"Carrito dispensado exitosamente. {cart.TotalItems} productos dispensados por un valor total de {cart.TotalValue:C}"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error dispensando carrito - Usuario: {UserId}", userId);

            return new CartResponse
            {
                Success = false,
                Message = "Error interno del servidor al dispensar el carrito"
            };
        }
    }

    public async Task<bool> CanUserDispenseCartAsync(int userId)
    {
        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            return cart?.CanBeConfirmed ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando si se puede dispensar carrito - Usuario: {UserId}", userId);
            return false;
        }
    }
}
