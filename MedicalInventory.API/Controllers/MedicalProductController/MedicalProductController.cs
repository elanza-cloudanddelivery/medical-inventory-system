using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Services.MedicalProductService;

namespace MedicalInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IMedicalProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMedicalProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // Obtener productos disponibles para dispensar 
        [HttpGet("available")]
        public async Task<ActionResult<MedicalProductResponse>> GetAvailableProducts()
        {
            try
            {
                if (GetUserId() <= 0)
                {
                    return Unauthorized(new MedicalProductResponse
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                _logger.LogInformation("Solicitando productos disponibles - Usuario: {UserId}, Rol: {Role}", GetUserId(), GetUserRole());

                var result = await _productService.GetAvailableProductsAsync(GetUserId(), GetUserRole());

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en endpoint GetAvailableProducts");
                return StatusCode(500, new MedicalProductResponse
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        // Buscar productos
        [HttpGet("search")]
        public async Task<ActionResult<MedicalProductResponse>> SearchProducts([FromQuery] MedicalProductFilterRequest filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter?.SearchTerm))
                {
                    return BadRequest(new MedicalProductResponse
                    {
                        Success = false,
                        Message = "El término de búsqueda es obligatorio"
                    });
                }

                var result = await _productService.SearchProductsAsync(filter, GetUserId(), GetUserRole());

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de productos - Término: {SearchTerm}", filter?.SearchTerm);
                return StatusCode(500, new MedicalProductResponse
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        // Obtener producto por ID
        [HttpGet("{productId}")]
        public async Task<ActionResult<MedicalProductDto>> GetProductById(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest("ID de producto inválido");
                }

                var product = await _productService.GetProductByIdAsync(productId, GetUserId(), GetUserRole());


                if (product == null)
                {
                    return NotFound($"Producto con ID {productId} no encontrado o no disponible");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo producto - ProductId: {ProductId}", productId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Obtener productos por categoría
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<List<MedicalProductDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest("ID de categoría inválido");
                }

                _logger.LogInformation("Productos por categoría - Usuario: {UserId}, Categoría: {CategoryId}", GetUserId(), categoryId);

                var products = await _productService.GetProductsByCategoryAsync(categoryId, GetUserId(), GetUserRole());

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo productos por categoría - CategoryId: {CategoryId}", categoryId);
                return StatusCode(500, "Error interno del servidor");
            }
        }


        // Obtener categorías disponibles (para filtros)
        [HttpGet("categories")]
        public ActionResult<List<object>> GetAvailableCategories()
        {
            try
            {
                var categories = Enum.GetValues<MedicalProductCategory>()
                    .Select(c => new
                    {
                        Id = (int)c,
                        Name = c.ToString(),
                        DisplayName = c.ToString()
                    })
                    .ToList();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo categorías");
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}