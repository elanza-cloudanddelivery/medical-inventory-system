using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;
using MedicalInventory.API.Models;

namespace MedicalInventory.API.Services.AuthService;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RfidLoginAsync(RfidLoginRequest request);
    Task<User?> ValidateUserCredentialsAsync(string identifier, string password);
    Task<User?> GetUserByRfidAsync(string rfidCode);
}
