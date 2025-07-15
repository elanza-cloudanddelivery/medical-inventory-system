using Microsoft.EntityFrameworkCore;
using MedicalInventory.API.Data;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models;
using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Services.JwtService;

namespace MedicalInventory.API.Services.AuthService
{
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
                _logger.LogInformation("Intento de login para identificador: {Identifier}", request.Identifier);

                var user = await ValidateUserCredentialsAsync(request.Identifier, request.Password);

                if (user == null)
                {
                    _logger.LogWarning("Login fallido - credenciales inválidas para: {Identifier}", request.Identifier);
                    return CreateErrorResponse("Credenciales inválidas");
                }

                return await ProcessSuccessfulLoginAsync(user, "Login exitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante el login para: {Identifier}", request.Identifier);
                return CreateErrorResponse("Error interno del servidor");
            }
        }

        public async Task<AuthResponse> RfidLoginAsync(RfidLoginRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de login RFID con código: {RfidCode}", request.RfidCode);

                var user = await GetUserByRfidAsync(request.RfidCode);

                if (user == null)
                {
                    _logger.LogWarning("Login RFID fallido - código no válido: {RfidCode}", request.RfidCode);
                    return CreateErrorResponse("Tarjeta RFID no válida o usuario inactivo");
                }

                return await ProcessSuccessfulLoginAsync(user, "Login RFID exitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante el login RFID para código: {RfidCode}", request.RfidCode);
                return CreateErrorResponse("Error interno del servidor");
            }
        }

        public async Task<User?> ValidateUserCredentialsAsync(string identifier, string password)
        {
            try
            {
                // ✅ 1. Buscar usuario por múltiples identificadores
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Username == identifier ||
                        u.Email == identifier ||
                        u.EmployeeId == identifier);

                if (user == null)
                {
                    _logger.LogDebug("Usuario no encontrado con identificador: {Identifier}", identifier);
                    return null;
                }

                // ✅ 2. Verificar estado de la cuenta ANTES de validar contraseña
                var accountValidation = ValidateUserAccount(user);
                if (!accountValidation.IsValid)
                {
                    _logger.LogWarning("Usuario {UserId} ({Username}) intentó login pero cuenta no válida: {Reason}",
                        user.Id, user.Username, accountValidation.ErrorMessage);
                    return null;
                }

                // ✅ 3. Validar contraseña
                // TODO: Implementar hash de contraseña (BCrypt o similar)
                // Por ahora comparación simple para desarrollo
                if (!VerifyPassword(user, password))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Username} (ID: {UserId})",
                        user.Username, user.Id);

                    await HandleFailedLoginAttemptAsync(user);
                    return null;
                }

                // ✅ 4. Login exitoso - resetear intentos fallidos
                if (user.FailedLoginAttempts > 0)
                {
                    await ResetFailedLoginAttemptsAsync(user);
                }

                _logger.LogInformation("Credenciales validadas correctamente para usuario: {Username} (ID: {UserId})",
                    user.Username, user.Id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando credenciales para: {Identifier}", identifier);
                return null;
            }
        }

        public async Task<User?> GetUserByRfidAsync(string rfidCode)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RfidCardCode == rfidCode);

                if (user == null)
                {
                    _logger.LogDebug("No se encontró usuario con código RFID: {RfidCode}", rfidCode);
                    return null;
                }

                // ✅ Validar estado de la cuenta para RFID
                var accountValidation = ValidateUserAccount(user);
                if (!accountValidation.IsValid)
                {
                    _logger.LogWarning("Usuario {UserId} ({Username}) intentó login RFID pero cuenta no válida: {Reason}",
                        user.Id, user.Username, accountValidation.ErrorMessage);
                    return null;
                }

                _logger.LogInformation("Usuario encontrado por RFID: {Username} (ID: {UserId})",
                    user.Username, user.Id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por RFID: {RfidCode}", rfidCode);
                return null;
            }
        }

        // ✅ Validaciones completas de la cuenta de usuario
        private (bool IsValid, string ErrorMessage) ValidateUserAccount(User user)
        {
            // 1. Verificar si el usuario está activo
            if (!user.IsActive)
            {
                _logger.LogWarning("Usuario inactivo intentó acceso: {Username} (ID: {UserId})",
                    user.Username, user.Id);
                return (false, "Cuenta inactiva. Contacte al administrador.");
            }

            // 2. Verificar si la cuenta está bloqueada temporalmente
            if (user.IsAccountLocked)
            {
                var lockTimeRemaining = user.AccountLockedUntil?.Subtract(DateTime.Now);
                if (lockTimeRemaining > TimeSpan.Zero)
                {
                    _logger.LogWarning("Usuario con cuenta bloqueada intentó acceso: {Username} (ID: {UserId}), Desbloqueada en: {Minutes} minutos",
                        user.Username, user.Id, lockTimeRemaining?.TotalMinutes);

                    return (false, $"Cuenta bloqueada temporalmente. Intente nuevamente en {lockTimeRemaining?.TotalMinutes:F0} minutos.");
                }
                else
                {
                    // La cuenta ya se desbloqueó automáticamente
                    _logger.LogInformation("Cuenta desbloqueada automáticamente para usuario: {Username} (ID: {UserId})",
                        user.Username, user.Id);
                }
            }

            // 3. Verificar si el usuario tiene un rol válido
            if (!Enum.IsDefined(typeof(UserRole), user.Role))
            {
                _logger.LogError("Usuario con rol inválido: {Username} (ID: {UserId}), Rol: {Role}",
                    user.Username, user.Id, user.Role);
                return (false, "Rol de usuario inválido. Contacte al administrador.");
            }

            // 4. Verificar si el departamento está asignado (si es requerido)
            if (string.IsNullOrEmpty(user.Department))
            {
                _logger.LogWarning("Usuario sin departamento asignado: {Username} (ID: {UserId})",
                    user.Username, user.Id);
                // No bloquear por esto, solo advertir
            }
        
            return (true, "Cuenta válida");
        }

        private bool VerifyPassword(User user, string password)
        {
            // TODO: Implementar verificación con hash (BCrypt)
            // Por ahora comparación simple para desarrollo
            bool isValid = user.PasswordHash == password;

            if (isValid)
            {
                _logger.LogDebug("Contraseña verificada correctamente para usuario: {Username}", user.Username);
            }
            else
            {
                _logger.LogDebug("Contraseña incorrecta para usuario: {Username}", user.Username);
            }

            return isValid;
        }

        private async Task HandleFailedLoginAttemptAsync(User user)
        {
            try
            {
                user.FailedLoginAttempts++;

                _logger.LogWarning("Intento de login fallido #{Attempts} para usuario: {Username} (ID: {UserId})",
                    user.FailedLoginAttempts, user.Username, user.Id);

                // ✅ Bloquear cuenta después de 5 intentos fallidos
                if (user.FailedLoginAttempts >= 5)
                {
                    user.AccountLockedUntil = DateTime.Now.AddMinutes(15);

                    _logger.LogError("Cuenta bloqueada por exceso de intentos fallidos - Usuario: {Username} (ID: {UserId}), Bloqueada hasta: {LockedUntil}",
                        user.Username, user.Id, user.AccountLockedUntil);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error manejando intento de login fallido para usuario: {UserId}", user.Id);
            }
        }

        private async Task ResetFailedLoginAttemptsAsync(User user)
        {
            try
            {
                var previousAttempts = user.FailedLoginAttempts;
                user.FailedLoginAttempts = 0;
                user.AccountLockedUntil = null;


                await _context.SaveChangesAsync();

                _logger.LogInformation("Intentos fallidos reseteados para usuario: {Username} (ID: {UserId}), Intentos anteriores: {PreviousAttempts}",
                    user.Username, user.Id, previousAttempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reseteando intentos fallidos para usuario: {UserId}", user.Id);
            }
        }

        private async Task<AuthResponse> ProcessSuccessfulLoginAsync(User user, string message)
        {
            try
            {
                // ✅ Validación final del acceso
                var accessValidation = ValidateUserAccess(user);
                if (!accessValidation.IsValid)
                {
                    _logger.LogWarning("Usuario {UserId} ({Username}) pasó autenticación pero falló validación de acceso: {Reason}",
                        user.Id, user.Username, accessValidation.ErrorMessage);
                    return CreateErrorResponse(accessValidation.ErrorMessage);
                }

                // ✅ Actualizar información de último login
                await UpdateLastLoginInfoAsync(user);

                // ✅ Generar token con claims completos
                var token = _jwtService.GenerateToken(user);
                var expiresAt = _jwtService.GetTokenExpiration();

                // ✅ Log de login exitoso con detalles de seguridad
                _logger.LogInformation("LOGIN EXITOSO - Usuario: {Username} (ID: {UserId}), Rol: {Role}, Departamento: {Department}, IP: {ClientIP}",
                    user.Username, user.Id, user.Role, user.Department, "TODO: Get client IP");

                return new AuthResponse
                {
                    Success = true,
                    Message = message,
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = CreateUserAuthDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando login exitoso para usuario: {UserId}", user.Id);
                return CreateErrorResponse("Error interno del servidor");
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateUserAccess(User user)
        {
            // Validaciones adicionales para el acceso
            if (!user.IsActive)
                return (false, "Cuenta inactiva");

            if (user.IsAccountLocked)
                return (false, "Cuenta bloqueada temporalmente");

            // ✅ Validar permisos específicos según el rol
            switch (user.Role)
            {
                case UserRole.Viewer:
                    // Los viewers solo pueden ver, no dispensar
                    break;
                case UserRole.Technician:
                    // Los técnicos tienen acceso limitado
                    break;
                case UserRole.Nurse:
                case UserRole.Doctor:
                case UserRole.Pharmacist:
                    // Personal médico con acceso completo
                    break;
                case UserRole.Administrator:
                case UserRole.SuperAdministrator:
                    // Administradores con acceso total
                    break;
                default:
                    _logger.LogError("Rol no reconocido para usuario: {Username} (ID: {UserId}), Rol: {Role}",
                        user.Username, user.Id, user.Role);
                    return (false, "Rol de usuario no válido");
            }

            return (true, "Acceso autorizado");
        }

        private async Task UpdateLastLoginInfoAsync(User user)
        {
            try
            {
                user.LastLoginAt = DateTime.Now;
                // TODO: Actualizar IP del cliente si es necesario
                // user.LastLoginIP = GetClientIP();

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando información de último login para usuario: {UserId}", user.Id);
                // No fallar el login por esto
            }
        }

        private AuthResponse CreateErrorResponse(string message)
        {
            return new AuthResponse
            {
                Success = false,
                Message = message,
                Token = string.Empty,
                ExpiresAt = DateTime.MinValue,
                User = null
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
}