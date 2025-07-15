using MedicalInventory.API.Data;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using MedicalInventory.API.Services.Validators.CartValidator;

namespace MedicalInventory.API.Services.CartService;

public class CartService : ICartService
{
    private readonly MedicalInventoryDbContext _context;
    private readonly ICartValidator _validator;
    private readonly ILogger<CartService> _logger;

    public CartService(
        MedicalInventoryDbContext context,
        ICartValidator validator,
        ILogger<CartService> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CartResponse> AddItemToCartAsync(int userId, AddToCartRequest request)
    {
        try
        {
            var validation = await _validator.ValidateAddItemAsync(userId, request);
            if (!validation.Success)
            {
                return new CartResponse { Success = false, Message = validation.Message };
            }

            var product = await _context.MedicalProducts.FindAsync(request.ProductId);
            var cart = await GetOrCreateActiveCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                var totalQuantity = existingItem.Quantity + request.Quantity;
                var totalValidation = _validator.ValidateProduct(product!, totalQuantity);
                if (!totalValidation.Success)
                {
                    return new CartResponse { Success = false, Message = $"No se puede agregar. {totalValidation.Message}" };
                }

                existingItem.Quantity = totalQuantity;
                if (!string.IsNullOrEmpty(request.ItemNotes))
                {
                    existingItem.ItemNotes = string.IsNullOrEmpty(existingItem.ItemNotes)
                        ? request.ItemNotes
                        : existingItem.ItemNotes + ". " + request.ItemNotes;
                }

                _logger.LogInformation("Cantidad actualizada en carrito - Usuario: {UserId}, Producto: {ProductName}, Nueva cantidad: {Quantity}",
                    userId, product!.Name, totalQuantity);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product!.Price,
                    AddedAt = DateTime.UtcNow,
                    ItemNotes = request.ItemNotes
                };

                cart.Items.Add(newItem);
                _context.CartItems.Add(newItem);

                _logger.LogInformation("Nuevo producto agregado al carrito - Usuario: {UserId}, Producto: {ProductName}, Cantidad: {Quantity}",
                    userId, product.Name, request.Quantity);
            }

            cart.LastModifiedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.Purpose)) cart.Purpose = request.Purpose;
            if (!string.IsNullOrEmpty(request.TargetDepartment)) cart.TargetDepartment = request.TargetDepartment;
            cart.Priority = request.Priority;

            await _context.SaveChangesAsync();

            var cartDto = await MapCartToDtoAsync(cart);

            return new CartResponse
            {
                Success = true,
                Message = "Producto agregado al carrito correctamente",
                Cart = cartDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error agregando item al carrito - Usuario: {UserId}, Producto: {ProductId}",
                userId, request.ProductId);

            return new CartResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<CartDto?> GetActiveCartAsync(int userId)
    {
        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null)
            {
                _logger.LogDebug("No se encontró carrito activo para usuario: {UserId}", userId);
                return null;
            }

            return await MapCartToDtoAsync(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo carrito activo - Usuario: {UserId}", userId);
            return null;
        }
    }

    public async Task<CartResponse> UpdateCartItemAsync(int userId, int itemId, int newQuantity)
    {
        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null)
            {
                return new CartResponse { Success = false, Message = "No se encontró carrito activo" };
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return new CartResponse { Success = false, Message = "Item no encontrado en el carrito" };
            }

            if (newQuantity <= 0)
            {
                return await RemoveItemFromCartAsync(userId, itemId);
            }

            var validation = _validator.ValidateProduct(item.Product!, newQuantity);
            if (!validation.Success)
            {
                return new CartResponse { Success = false, Message = validation.Message };
            }

            var oldQuantity = item.Quantity;
            item.Quantity = newQuantity;
            cart.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cantidad de item actualizada - Usuario: {UserId}, Producto: {ProductName}, Cantidad anterior: {OldQuantity}, Nueva: {NewQuantity}",
                userId, item.Product?.Name, oldQuantity, newQuantity);

            var cartDto = await MapCartToDtoAsync(cart);

            return new CartResponse
            {
                Success = true,
                Message = "Cantidad actualizada correctamente",
                Cart = cartDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando item del carrito - Usuario: {UserId}, Item: {ItemId}",
                userId, itemId);

            return new CartResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<CartResponse> RemoveItemFromCartAsync(int userId, int itemId)
    {
        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null)
            {
                return new CartResponse { Success = false, Message = "No se encontró carrito activo" };
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return new CartResponse { Success = false, Message = "Item no encontrado en el carrito" };
            }

            var productName = item.Product?.Name ?? "Producto desconocido";
            cart.Items.Remove(item);
            _context.CartItems.Remove(item);
            cart.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Item eliminado del carrito - Usuario: {UserId}, Producto: {ProductName}",
                userId, productName);

            var cartDto = await MapCartToDtoAsync(cart);

            return new CartResponse
            {
                Success = true,
                Message = "Item eliminado del carrito correctamente",
                Cart = cartDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando item del carrito - Usuario: {UserId}, Item: {ItemId}",
                userId, itemId);

            return new CartResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<CartResponse> ClearCartAsync(int userId)
    {
        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

            if (cart == null)
            {
                return new CartResponse { Success = false, Message = "No se encontró carrito activo" };
            }

            var itemCount = cart.Items.Count;
            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Carrito limpiado - Usuario: {UserId}, Items eliminados: {ItemCount}",
                userId, itemCount);

            return new CartResponse
            {
                Success = true,
                Message = "Carrito limpiado correctamente",
                Cart = await MapCartToDtoAsync(cart)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error limpiando carrito - Usuario: {UserId}", userId);
            return new CartResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    private async Task<Cart> GetOrCreateActiveCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                Status = CartStatus.Active,
                Priority = DispensationPriority.Normal
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nuevo carrito creado - Usuario: {UserId}, CartId: {CartId}", userId, cart.Id);
        }

        return cart;
    }

    private async Task<CartDto> MapCartToDtoAsync(Cart cart)
    {
        if (cart.Items.Any() && cart.Items.First().Product == null)
        {
            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Product)
                .LoadAsync();
        }

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Status = cart.StatusDescription,
            StatusSummary = cart.StatusSummary,
            Priority = cart.PriorityDescription,
            Purpose = cart.Purpose,
            TargetDepartment = cart.TargetDepartment,
            Notes = cart.Notes,
            CreatedAt = cart.CreatedAt,
            LastModifiedAt = cart.LastModifiedAt,
            ConfirmedAt = cart.ConfirmedAt,
            TotalItems = cart.TotalItems,
            TotalQuantity = cart.TotalQuantity,
            TotalValue = cart.TotalValue,
            IsEmpty = cart.IsEmpty,
            CanBeConfirmed = cart.CanBeConfirmed,
            HasProblems = cart.ProblematicItemsCount > 0,
            ProblematicItemsCount = cart.ProblematicItemsCount,
            ControlledProductsCount = cart.ControlledProductsCount,
            ExpiredProductsCount = cart.ExpiredProductsCount,
            InsufficientStockCount = cart.InsufficientStockCount,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Producto desconocido",
                ProductSKU = i.Product?.SKU ?? "",
                ProductCategory = i.Product?.Category.ToString() ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                AddedAt = i.AddedAt,
                ItemNotes = i.ItemNotes,
                ItemStatus = i.ItemStatus,
                CanBeDispensed = i.CanBeDispensed,
                HasSufficientStock = i.HasSufficientStock,
                IsProductExpired = i.IsProductExpired,
                IsControlledProduct = i.IsControlledProduct,
                RequiresAuthorization = i.RequiresAuthorization,
                IsNearExpiration = i.IsNearExpiration,
                WouldTriggerReorder = i.WouldTriggerReorder,
                AvailableStock = i.Product?.Stock ?? 0,
                MinimumStock = i.Product?.MinimumStock ?? 0,
                ExpirationDate = i.Product?.ExpirationDate,
                BatchNumber = i.Product?.BatchNumber
            }).ToList()
        };
    }
}

