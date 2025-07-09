using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using MedicalInventory.API.Models;

namespace MedicalInventory.API.Services.JwtService;
public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            try
            {
                var jwtKey = _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
                var jwtIssuer = _configuration["JWT:Issuer"] ?? "MedicalInventoryAPI";
                var jwtAudience = _configuration["JWT:Audience"] ?? "MedicalInventoryClient";
                var expirationHours = int.Parse(_configuration["JWT:ExpirationHours"] ?? "24");

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("RoleCode", ((int)user.Role).ToString()),
                    new Claim("Department", user.Department),
                    new Claim("EmployeeId", user.EmployeeId),
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("IsMedicalStaff", user.IsMedicalStaff.ToString()),
                    new Claim("CanAccessControlledProducts", user.CanAccessControlledProducts.ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.Now.AddHours(expirationHours);

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando JWT token para usuario {UserId}", user.Id);
                throw;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtKey = _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
                var jwtIssuer = _configuration["JWT:Issuer"] ?? "MedicalInventoryAPI";
                var jwtAudience = _configuration["JWT:Audience"] ?? "MedicalInventoryClient";

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validando JWT token");
                return null;
            }
        }

        public DateTime GetTokenExpiration()
        {
            var expirationHours = int.Parse(_configuration["JWT:ExpirationHours"] ?? "24");
            return DateTime.Now.AddHours(expirationHours);
        }

        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }