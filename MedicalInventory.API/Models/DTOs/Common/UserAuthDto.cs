namespace MedicalInventory.API.Models.DTOs.Common; 
public class UserAuthDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleCode { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        
        // Permisos calculados para la UI
        public bool CanDispenseNormalProducts { get; set; }
        public bool CanDispenseControlledProducts { get; set; }
        public bool IsMedicalStaff { get; set; }
    }