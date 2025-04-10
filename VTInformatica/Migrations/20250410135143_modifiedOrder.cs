using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class modifiedOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "405e0839-bcea-4a52-9261-dd4e2c63a4bb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "59edc8cb-f007-4cd8-aed9-be3e682601a4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bce986d1-1f38-4aa9-bd2b-379a578ae9ea");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "67c46ec5-a54b-4176-be0d-bc0b4939e321", "67c46ec5-a54b-4176-be0d-bc0b4939e321", "Customer", "CUSTOMER" },
                    { "a3653b57-e07b-48af-a102-f0778f5de01d", "a3653b57-e07b-48af-a102-f0778f5de01d", "Seller", "SELLER" },
                    { "f3784662-a768-4d46-b012-97509d6681e5", "f3784662-a768-4d46-b012-97509d6681e5", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "67c46ec5-a54b-4176-be0d-bc0b4939e321");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a3653b57-e07b-48af-a102-f0778f5de01d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f3784662-a768-4d46-b012-97509d6681e5");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "405e0839-bcea-4a52-9261-dd4e2c63a4bb", "405e0839-bcea-4a52-9261-dd4e2c63a4bb", "Admin", "ADMIN" },
                    { "59edc8cb-f007-4cd8-aed9-be3e682601a4", "59edc8cb-f007-4cd8-aed9-be3e682601a4", "Seller", "SELLER" },
                    { "bce986d1-1f38-4aa9-bd2b-379a578ae9ea", "bce986d1-1f38-4aa9-bd2b-379a578ae9ea", "Customer", "CUSTOMER" }
                });
        }
    }
}
