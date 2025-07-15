using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MedicalInventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreTestProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MedicalProducts",
                columns: new[] { "Id", "BatchNumber", "Category", "ExpirationDate", "IsControlled", "ManufacturingDate", "MinimumStock", "Name", "Price", "RequiresAuthorization", "RfidCode", "SKU", "Stock", "StorageConditions" },
                values: new object[,]
                {
                    { 4, "BATCH-2024-004", 2, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, "Paracetamol 500mg", 3.25m, false, "RFID-PARA-001", "PARA-001", 200, "Ambiente seco, temperatura ambiente" },
                    { 5, "BATCH-2024-005", 2, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25, "Ibuprofeno 400mg", 4.80m, false, "RFID-IBU-001", "IBU-001", 150, "Ambiente seco" },
                    { 6, "BATCH-2024-006", 2, new DateTime(2026, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Fentanilo 100mcg", 25.00m, true, "RFID-FENT-001", "FENT-001", 20, "Refrigerado 2-8°C, caja fuerte" },
                    { 7, "BATCH-2024-007", 1, new DateTime(2028, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Tijeras Quirúrgicas Curvas", 15.75m, true, "RFID-TIJ-001", "TIJ-001", 25, "Ambiente seco, esterilizado" },
                    { 8, "BATCH-2024-008", 1, new DateTime(2027, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2023, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, "Pinzas Hemostáticas", 12.30m, true, "RFID-PIN-001", "PIN-001", 40, "Esterilizado" },
                    { 9, "BATCH-2024-009", 3, new DateTime(2027, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 50, "Vendas Elásticas 10cm", 2.85m, false, "RFID-VEND-001", "VEND-001", 300, "Ambiente seco" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MedicalProducts",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
