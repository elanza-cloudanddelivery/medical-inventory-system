using Microsoft.AspNetCore.Mvc;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Services.AuthService;

namespace MedicalInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Datos de entrada inválidos" 
                });
            }

            var result = await _authService.LoginAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint de login");
            return StatusCode(500, new AuthResponse 
            { 
                Success = false, 
                Message = "Error interno del servidor" 
            });
        }
    }

    [HttpPost("login/rfid")]
    public async Task<ActionResult<AuthResponse>> RfidLogin([FromBody] RfidLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Código RFID inválido" 
                });
            }

            var result = await _authService.RfidLoginAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint de login RFID");
            return StatusCode(500, new AuthResponse 
            { 
                Success = false, 
                Message = "Error interno del servidor" 
            });
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { 
            message = "Auth API funcionando correctamente", 
            timestamp = DateTime.Now,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }
}