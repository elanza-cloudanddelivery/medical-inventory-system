using System.Security.Claims;
using MedicalInventory.API.Models;

namespace MedicalInventory.API.Services.JwtService;

public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        DateTime GetTokenExpiration();
        int GetCurrentUserId(ClaimsPrincipal user);
    }