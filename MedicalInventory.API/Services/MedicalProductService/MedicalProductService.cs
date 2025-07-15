using MedicalInventory.API.Data;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace MedicalInventory.API.Services.MedicalProductService;

public class MedicalProductService : IMedicalProductService
{
    private readonly MedicalInventoryDbContext _context;
    private readonly ILogger<MedicalProductService> _logger;

    public MedicalProductService(MedicalInventoryDbContext context, ILogger<MedicalProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MedicalProductResponse> GetAvailableProductsAsync(int userId, string userRole)
    {
        try
        {
            var query = _context.MedicalProducts
            .Where(p => p.Stock > 0 && p.ExpirationDate > DateTime.Now);

            query = FilterByUserRole(query, userRole);

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            var productDtos = products.Select(p => MapToDto(p, userRole)).ToList();

            _logger.LogInformation("Productos obtenidos - Usuario: {UserId}, Rol: {Role}, Total: {Count}",
                userId, userRole, productDtos.Count);

            return new MedicalProductResponse
            {
                Success = true,
                Message = "Productos obtenidos correctamente",
                Products = productDtos,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo productos - Usuario: {UserId}", userId);

            return new MedicalProductResponse
            {
                Success = false,
                Message = "Error interno del servidor"
            };
        }
    }

    public async Task<MedicalProductResponse> SearchProductsAsync(MedicalProductFilterRequest filter, int userId, string userRole)
    {
        try
        {

            var query = _context.MedicalProducts.AsQueryable();

            query = FilterByUserRole(query, userRole);

            query = ApplyFilters(query, filter);

            var products = await query
                .OrderBy(p => p.Name)
                .Take(25) 
                .ToListAsync();

            var productDtos = products.Select(p => MapToDto(p, userRole)).ToList();

            return new MedicalProductResponse
            {
                Success = true,
                Message = $"Se encontraron {productDtos.Count} productos",
                Products = productDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda de productos");
            return new MedicalProductResponse { Success = false, Message = "Error interno del servidor" };
        }
    }

    public async Task<MedicalProductDto?> GetProductByIdAsync(int productId, int userId, string userRole)
    {
        try
        {
            var product = await _context.MedicalProducts.FindAsync(productId);

            if (product == null || product.IsExpired || product.Stock <= 0)
            {
                return null;
            }

            if (!CanUserAccessProduct(product, userRole))
            {
                _logger.LogWarning("Usuario sin permisos intentó acceder a producto - Usuario: {UserId}, Producto: {ProductId}",
                    userId, productId);
                return null;
            }

            return MapToDto(product, userRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo producto por ID - ProductId: {ProductId}", productId);
            return null;
        }
    }

    public async Task<List<MedicalProductDto>> GetProductsByCategoryAsync(int categoryId, int userId, string userRole)
    {
        try
        {
            var query = _context.MedicalProducts
                .Where(p => (int)p.Category == categoryId && p.Stock > 0 && !p.IsExpired);

            query = FilterByUserRole(query, userRole);

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(p => MapToDto(p, userRole)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo productos por categoría - CategoryId: {CategoryId}", categoryId);
            return new List<MedicalProductDto>();
        }
    }

    // METODOS AUXILIARES //    
    // METODOS AUXILIARES //
    // METODOS AUXILIARES //
    // METODOS AUXILIARES //
    private IQueryable<MedicalProduct> FilterByUserRole(IQueryable<MedicalProduct> query, string userRole)
    {
        return userRole switch
        {
            "Technician" or "Viewer" =>
                query.Where(p => !p.RequiresAuthorization && !p.IsControlled),

            "Nurse" =>
                query.Where(p => !p.IsControlled),

            "Doctor" or "Pharmacist" or "Administrator" or "SuperAdministrator" =>
                query, // Pueden ver todos

            _ => query.Where(p => false) // Sin rol = sin productos
        };
    }

    private IQueryable<MedicalProduct> ApplyFilters(IQueryable<MedicalProduct> query, MedicalProductFilterRequest filter)
    {
        if (filter == null) return query;

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.SKU.ToLower().Contains(searchLower)
            );
        }
        return query;
    }

    private bool CanUserAccessProduct(MedicalProduct product, string userRole)
    {
        return userRole switch
        {
            "Technician" or "Viewer" => !product.RequiresAuthorization && !product.IsControlled,
            "Nurse" => !product.IsControlled,
            "Doctor" or "Pharmacist" or "Administrator" or "SuperAdministrator" => true,
            _ => false
        };
    }

    private MedicalProductDto MapToDto(MedicalProduct product, string userRole)
    {
        return new MedicalProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Category = product.Category.ToString(),
            CategoryCode = (int)product.Category,
            Price = product.Price,
            Stock = product.Stock,
            MinimumStock = product.MinimumStock,
            RfidCode = product.RfidCode,
            ExpirationDate = product.ExpirationDate,
            ManufacturingDate = product.ManufacturingDate,
            BatchNumber = product.BatchNumber,
            RequiresAuthorization = product.RequiresAuthorization,
            IsControlled = product.IsControlled,
            StorageConditions = product.StorageConditions,
            IsNearExpiration = product.IsNearExpiration,
            IsExpired = product.IsExpired,
            IsLowStock = product.IsLowStock,
            IsAvailable = product.IsAvailable,
            CanBeAddedToCart = CanUserAccessProduct(product, userRole),
        };
    }

}
