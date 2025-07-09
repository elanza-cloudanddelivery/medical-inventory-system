using MedicalInventory.API.Models.DTOs.Common;

namespace MedicalInventory.API.Models.DTOs.Response;
public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserAuthDto? User { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
    