using Microsoft.EntityFrameworkCore;
using MedicalInventory.API.Data;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Services.JwtService;

namespace MedicalInventory.API.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly MedicalInventoryDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        MedicalInventoryDbContext context,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await ValidateUserCredentialsAsync(request.Identifier, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Intento de login fallido para: {Identifier}", request.Identifier);
                return CreateErrorResponse("Credenciales inválidas");
            }

            return await ProcessSuccessfulLoginAsync(user, "Login exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login para: {Identifier}", request.Identifier);
            return CreateErrorResponse("Error interno del servidor");
        }
    }

    public async Task<AuthResponse> RfidLoginAsync(RfidLoginRequest request)
    {
        try
        {
            var user = await GetUserByRfidAsync(request.RfidCode);
            
            if (user == null)
            {
                _logger.LogWarning("Intento de login RFID fallido para código: {RfidCode}", request.RfidCode);
                return CreateErrorResponse("Tarjeta RFID no válida");
            }

            return await ProcessSuccessfulLoginAsync(user, "Login RFID exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login RFID para código: {RfidCode}", request.RfidCode);
            return CreateErrorResponse("Error interno del servidor");
        }
    }

    public async Task<User?> ValidateUserCredentialsAsync(string identifier, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => 
                u.Username == identifier || 
                u.Email == identifier || 
                u.EmployeeId == identifier);

        if (user == null)
            return null;

        // TODO: Implementar hash de contraseña (BCrypt o similar)
        // Por ahora comparación simple para desarrollo
        if (user.PasswordHash == password)
        {
            // Login exitoso - resetear intentos fallidos
            if (user.FailedLoginAttempts > 0)
            {
                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;
                await _context.SaveChangesAsync();
            }
            return user;
        }

        // Login fallido - incrementar intentos
        await HandleFailedLoginAttemptAsync(user);
        return null;
    }

    public async Task<User?> GetUserByRfidAsync(string rfidCode)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RfidCardCode == rfidCode && u.IsActive);
    }

    private async Task<AuthResponse> ProcessSuccessfulLoginAsync(User user, string message)
    {
        var validationResult = ValidateUserAccess(user);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Usuario {UserId} intentó acceder pero: {Reason}", user.Id, validationResult.ErrorMessage);
            return CreateErrorResponse(validationResult.ErrorMessage);
        }

        // Actualizar último login
        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        // Generar token
        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetTokenExpiration();

        _logger.LogInformation("{Message} para usuario: {Username}", message, user.Username);

        return new AuthResponse
        {
            Success = true,
            Message = message,
            Token = token,
            ExpiresAt = expiresAt,
            User = CreateUserAuthDto(user)
        };
    }

    private (bool IsValid, string ErrorMessage) ValidateUserAccess(User user)
    {
        if (!user.IsActive)
            return (false, "Cuenta inactiva");
        
        if (user.IsAccountLocked)
            return (false, "Cuenta bloqueada temporalmente");
        
        return (true, "");
    }

    private async Task HandleFailedLoginAttemptAsync(User user)
    {
        user.FailedLoginAttempts++;
        
        // Bloquear cuenta después de 5 intentos fallidos
        if (user.FailedLoginAttempts >= 5)
        {
            user.AccountLockedUntil = DateTime.Now.AddMinutes(15);
            _logger.LogWarning("Usuario {UserId} bloqueado por exceso de intentos fallidos", user.Id);
        }
        
        await _context.SaveChangesAsync();
    }

    private AuthResponse CreateErrorResponse(string message)
    {
        return new AuthResponse
        {
            Success = false,
            Message = message
        };
    }

    private UserAuthDto CreateUserAuthDto(User user)
    {
        return new UserAuthDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleCode = (int)user.Role,
            RoleName = user.Role.ToString(),
            Department = user.Department,
            CanDispenseNormalProducts = user.IsMedicalStaff,
            CanDispenseControlledProducts = user.CanAccessControlledProducts,
            IsMedicalStaff = user.IsMedicalStaff
        };
    }
}
