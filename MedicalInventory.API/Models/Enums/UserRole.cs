    public enum UserRole
    {
        // Médico: Acceso completo a dispensación y productos controlados
        Doctor = 1,
        
        // Enfermero: Acceso a dispensación de productos no controlados
        Nurse = 2,
        
        // Farmacéutico: Acceso completo incluyendo gestión de medicamentos
        Pharmacist = 3,
        
        // Técnico: Acceso limitado para mantenimiento y soporte
        Technician = 4,
        
        // Administrador: Gestión completa del sistema excepto configuración crítica
        Administrator = 5,
        
        // Super Administrador: Acceso total al sistema
        SuperAdministrator = 6,
        
        // Solo Lectura: Puede ver reportes pero no realizar movimientos
        Viewer = 7
    }