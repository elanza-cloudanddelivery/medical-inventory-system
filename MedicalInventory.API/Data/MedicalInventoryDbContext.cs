using Microsoft.EntityFrameworkCore;
using MedicalInventory.API.Models;

namespace MedicalInventory.API.Data
{
    public class MedicalInventoryDbContext : DbContext
    {
        public MedicalInventoryDbContext(DbContextOptions<MedicalInventoryDbContext> options)
            : base(options)
        {
        }

        // === DbSets - TABLAS DE LA BASE DE DATOS ===
        public DbSet<MedicalProduct> MedicalProducts { get; set; }
        public DbSet<MedicalProductMovement> MedicalProductMovements { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CONFIGURACIÓN DE MEDICALPRODUCT ===
            modelBuilder.Entity<MedicalProduct>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasIndex(p => p.SKU)
                    .IsUnique()
                    .HasDatabaseName("IX_MedicalProducts_SKU");

                entity.HasIndex(p => p.RfidCode)
                    .IsUnique()
                    .HasDatabaseName("IX_MedicalProducts_RfidCode")
                    .HasFilter("[RfidCode] IS NOT NULL");

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.Property(p => p.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(p => p.SKU)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(p => p.Category)
                    .HasConversion<int>()
                    .IsRequired();
            });

            // === CONFIGURACIÓN DE USER ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.HasIndex(u => u.RfidCardCode)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_RfidCardCode")
                    .HasFilter("[RfidCardCode] IS NOT NULL");

                entity.Property(u => u.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(u => u.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasConversion<int>()
                    .IsRequired();
            });

            // === CONFIGURACIÓN DE CART ===
            modelBuilder.Entity<Cart>(entity =>
            {

                entity.HasKey(c => c.Id);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.LastModifiedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.Status)
                    .IsRequired();

                entity.Property(c => c.Purpose)
                    .HasMaxLength(500);

                entity.Property(c => c.TargetDepartment)
                    .HasMaxLength(100);

                entity.Property(c => c.Notes)
                    .HasMaxLength(1000);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Carts) 
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Items)
                    .WithOne(i => i.Cart)
                    .HasForeignKey(i => i.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

            });


            // === CONFIGURACIÓN DE CARTITEM ===
            modelBuilder.Entity<CartItem>(entity =>
            {
               
                entity.HasKey(ci => ci.Id);

                entity.Property(ci => ci.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ci => ci.ItemNotes)
                    .HasMaxLength(500);

                entity.Property(ci => ci.AddedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(ci => ci.Product)
                    .WithMany() 
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
    

            // === CONFIGURACIÓN DE PRODUCTMOVEMENT ===
            modelBuilder.Entity<MedicalProductMovement>(entity =>
            {
                entity.HasKey(pm => pm.Id);

                // Relación ProductMovement -> MedicalProduct
                entity.HasOne(pm => pm.Product)
                    .WithMany(p => p.Movements)
                    .HasForeignKey(pm => pm.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación ProductMovement -> User
                entity.HasOne(pm => pm.User)
                    .WithMany(u => u.ProductMovements)
                    .HasForeignKey(pm => pm.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pm => pm.UnitCost)
                    .HasPrecision(18, 2);

                entity.Property(pm => pm.Type)
                    .HasConversion<int>()
                    .IsRequired();

                entity.HasIndex(pm => pm.ProductId)
                    .HasDatabaseName("IX_ProductMovements_ProductId");

                entity.HasIndex(pm => pm.UserId)
                    .HasDatabaseName("IX_ProductMovements_UserId");

                entity.HasIndex(pm => pm.Timestamp)
                    .HasDatabaseName("IX_ProductMovements_Timestamp");
            });

            // === DATOS SEMILLA CON FECHAS FIJAS ===
            // IMPORTANTE: Usar fechas fijas en lugar de DateTime.Now para evitar el error

            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "admin123",
                    FullName = "Administrador del Sistema",
                    Email = "admin@hospital.com",
                    Role = UserRole.Administrator,
                    Department = "Administración",
                    IsActive = true,
                    CreatedAt = baseDate
                },
                new User
                {
                    Id = 2,
                    Username = "doctor.garcia",
                    PasswordHash = "doctor123",
                    FullName = "Dr. Carlos García",
                    Email = "c.garcia@hospital.com",
                    Role = UserRole.Doctor,
                    Department = "Cardiología",
                    IsActive = true,
                    CreatedAt = baseDate
                },
                new User
                {
                    Id = 3,
                    Username = "enfermera.lopez",
                    PasswordHash = "nurse123",
                    FullName = "María López",
                    Email = "m.lopez@hospital.com",
                    Role = UserRole.Nurse,
                    Department = "Urgencias",
                    IsActive = true,
                    CreatedAt = baseDate
                }
            );

            modelBuilder.Entity<MedicalProduct>().HasData(
                new MedicalProduct
                {
                    Id = 1,
                    Name = "Bisturí Desechable Estéril",
                    SKU = "BIST-001",
                    Category = MedicalProductCategory.InstrumentalQuirurgico,
                    Price = 2.50m,
                    Stock = 100,
                    MinimumStock = 20,
                    RfidCode = "RFID-BIST-001",
                    ExpirationDate = new DateTime(2026, 1, 1),
                    ManufacturingDate = new DateTime(2023, 7, 1),
                    BatchNumber = "BATCH-2024-001",
                    RequiresAuthorization = false,
                    IsControlled = false,
                    StorageConditions = "Ambiente seco, temperatura ambiente"
                },
                new MedicalProduct
                {
                    Id = 2,
                    Name = "Gasas Estériles 10x10cm",
                    SKU = "GASA-001",
                    Category = MedicalProductCategory.MaterialCuracion,
                    Price = 0.75m,
                    Stock = 500,
                    MinimumStock = 100,
                    RfidCode = "RFID-GASA-001",
                    ExpirationDate = new DateTime(2025, 7, 1),
                    ManufacturingDate = new DateTime(2023, 10, 1),
                    BatchNumber = "BATCH-2024-002",
                    RequiresAuthorization = false,
                    IsControlled = false,
                    StorageConditions = "Ambiente seco"
                },
                new MedicalProduct
                {
                    Id = 3,
                    Name = "Morfina 10mg/ml",
                    SKU = "MORF-001",
                    Category = MedicalProductCategory.Medicamentos,
                    Price = 15.00m,
                    Stock = 50,
                    MinimumStock = 10,
                    RfidCode = "RFID-MORF-001",
                    ExpirationDate = new DateTime(2025, 1, 1),
                    ManufacturingDate = new DateTime(2023, 11, 1),
                    BatchNumber = "BATCH-2024-003",
                    RequiresAuthorization = true,
                    IsControlled = true,
                    StorageConditions = "Refrigerado 2-8°C, almacén seguro"
                }
            );
        }
    }
}