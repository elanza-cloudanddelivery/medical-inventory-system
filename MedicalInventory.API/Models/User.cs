using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalInventory.API.Models
{
    // Esta clase representa un usuario del sistema médico
    // Puede ser un médico, enfermero, administrador, etc.
    public class User
    {
        // Identificador único del usuario
        public int Id { get; set; }
        
        // Nombre de usuario para login (debe ser único)
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public required string Username { get; set; }
        
        // Contraseña hasheada (NUNCA almacenar contraseñas en texto plano)
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public required string PasswordHash { get; set; }
        
        // Nombre completo del usuario
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder 100 caracteres")]
        public required string FullName { get; set; }
        
        // Email del usuario (usado para notificaciones y recuperación de contraseña)
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
        public required string Email { get; set; }
        
        // Rol del usuario en el sistema (define qué puede hacer)
        [Required(ErrorMessage = "El rol es obligatorio")]
        public UserRole Role { get; set; }
        
        // Departamento al que pertenece el usuario
        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public required string Department { get; set; }
        
        // Número de empleado o identificación profesional
        [StringLength(50)]
        public string? EmployeeId { get; set; }
        
        // Código RFID de la tarjeta del usuario para acceso rápido
        [StringLength(100)]
        public string? RfidCardCode { get; set; }
        
        // Teléfono de contacto (opcional pero útil para emergencias)
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        // Indica si el usuario está activo en el sistema
        public bool IsActive { get; set; } = true;
        
        // Fecha de creación de la cuenta
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Fecha del último acceso al sistema
        public DateTime? LastLoginAt { get; set; }
        
        // Número de intentos de login fallidos (para seguridad)
        public int FailedLoginAttempts { get; set; } = 0;
        
        // Fecha hasta la cual la cuenta está bloqueada (si aplica)
        public DateTime? AccountLockedUntil { get; set; }
        
        // Navegación: Lista de todos los movimientos realizados por este usuario
        // Esto nos permite ver todo el historial de actividad de un usuario específico
        public List<MedicalProductMovement> ProductMovements { get; set; } = new List<MedicalProductMovement>();
        
        // Navegación: Lista de carritos de compra creados por este usuario
        public List<Cart> Carts { get; set; } = new List<Cart>();
        
        // Propiedades calculadas para facilitar el manejo del usuario
        
        // Indica si la cuenta del usuario está bloqueada
        public bool IsAccountLocked => AccountLockedUntil.HasValue && AccountLockedUntil > DateTime.Now;
        
        // Indica si el usuario puede acceder al sistema
        public bool CanAccess => IsActive && !IsAccountLocked;
        
        // Indica si el usuario tiene permisos administrativos
        public bool IsAdministrator => Role == UserRole.Administrator || Role == UserRole.SuperAdministrator;
        
        // Indica si el usuario es personal médico (puede dispensar productos)
        public bool IsMedicalStaff => Role == UserRole.Doctor || 
                                     Role == UserRole.Nurse || 
                                     Role == UserRole.Pharmacist;
        
        // Indica si el usuario puede acceder a productos controlados
        public bool CanAccessControlledProducts => Role == UserRole.Doctor || 
                                                  Role == UserRole.Pharmacist || 
                                                  Role == UserRole.Administrator;
        
        // Calcula cuántos días han pasado desde el último login
        public int DaysSinceLastLogin => LastLoginAt.HasValue 
            ? (DateTime.Now - LastLoginAt.Value).Days 
            : int.MaxValue;
        
        // Descripción del rol para mostrar en interfaces
        public string RoleDescription => Role switch
        {
            UserRole.Doctor => "Médico",
            UserRole.Nurse => "Enfermero/a",
            UserRole.Pharmacist => "Farmacéutico/a",
            UserRole.Technician => "Técnico",
            UserRole.Administrator => "Administrador",
            UserRole.SuperAdministrator => "Super Administrador",
            UserRole.Viewer => "Solo Lectura",
            _ => "Rol Desconocido"
        };
    }
    
    // Enumeración que define todos los roles posibles en el sistema médico

}