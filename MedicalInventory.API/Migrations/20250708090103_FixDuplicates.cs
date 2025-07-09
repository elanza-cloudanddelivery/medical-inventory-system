using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MedicalInventory.API.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumStock = table.Column<int>(type: "INTEGER", nullable: false),
                    RfidCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RequiresAuthorization = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsControlled = table.Column<bool>(type: "INTEGER", nullable: false),
                    StorageConditions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RfidCardCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountLockedUntil = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TargetDepartment = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalProductMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsAutomated = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalProductMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalProductMovements_MedicalProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "MedicalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalProductMovements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CartId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    ItemNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_MedicalProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "MedicalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MedicalProducts",
                columns: new[] { "Id", "BatchNumber", "Category", "ExpirationDate", "IsControlled", "ManufacturingDate", "MinimumStock", "Name", "Price", "RequiresAuthorization", "RfidCode", "SKU", "Stock", "StorageConditions" },
                values: new object[,]
                {
                    { 1, "BATCH-2024-001", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, "Bisturí Desechable Estéril", 2.50m, false, "RFID-BIST-001", "BIST-001", 100, "Ambiente seco, temperatura ambiente" },
                    { 2, "BATCH-2024-002", 3, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 100, "Gasas Estériles 10x10cm", 0.75m, false, "RFID-GASA-001", "GASA-001", 500, "Ambiente seco" },
                    { 3, "BATCH-2024-003", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, "Morfina 10mg/ml", 15.00m, true, "RFID-MORF-001", "MORF-001", 50, "Refrigerado 2-8°C, almacén seguro" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountLockedUntil", "CreatedAt", "Department", "Email", "EmployeeId", "FailedLoginAttempts", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "PhoneNumber", "RfidCardCode", "Role", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administración", "admin@hospital.com", null, 0, "Administrador del Sistema", true, null, "admin123", null, null, 5, "admin" },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cardiología", "c.garcia@hospital.com", null, 0, "Dr. Carlos García", true, null, "doctor123", null, null, 1, "doctor.garcia" },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Urgencias", "m.lopez@hospital.com", null, 0, "María López", true, null, "nurse123", null, null, 2, "enfermera.lopez" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_ProductId",
                table: "MedicalProductMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_Timestamp",
                table: "MedicalProductMovements",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_UserId",
                table: "MedicalProductMovements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalProducts_RfidCode",
                table: "MedicalProducts",
                column: "RfidCode",
                unique: true,
                filter: "[RfidCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalProducts_SKU",
                table: "MedicalProducts",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RfidCardCode",
                table: "Users",
                column: "RfidCardCode",
                unique: true,
                filter: "[RfidCardCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "MedicalProductMovements");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "MedicalProducts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
