using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.DTOs.Request;

    public class LoginRequest
    {
        [Required(ErrorMessage = "El identificador es obligatorio")]
        public string Identifier { get; set; } = string.Empty; // Username, Email o EmployeeId
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }