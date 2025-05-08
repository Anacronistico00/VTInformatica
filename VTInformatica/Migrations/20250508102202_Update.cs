using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Manufacturers_ManufacturerId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0ddaa4f4-bde9-4844-8e00-9671f09fc6b8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "70d6b622-4270-41af-82ff-0cd1265e69c7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d8684d8-6135-4965-b9cb-e50eaf6f02f8");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "98b4e551-a53d-40fc-9fa4-29d6dd582481", "98b4e551-a53d-40fc-9fa4-29d6dd582481", "Admin", "ADMIN" },
                    { "9ab947f2-b737-46bf-a388-2f8962573a2b", "9ab947f2-b737-46bf-a388-2f8962573a2b", "Seller", "SELLER" },
                    { "bd7ef491-0f1b-4b8c-9c80-340e14cfac9b", "bd7ef491-0f1b-4b8c-9c80-340e14cfac9b", "Customer", "CUSTOMER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Manufacturers_ManufacturerId",
                table: "Products",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Manufacturers_ManufacturerId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "98b4e551-a53d-40fc-9fa4-29d6dd582481");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ab947f2-b737-46bf-a388-2f8962573a2b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd7ef491-0f1b-4b8c-9c80-340e14cfac9b");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0ddaa4f4-bde9-4844-8e00-9671f09fc6b8", "0ddaa4f4-bde9-4844-8e00-9671f09fc6b8", "Customer", "CUSTOMER" },
                    { "70d6b622-4270-41af-82ff-0cd1265e69c7", "70d6b622-4270-41af-82ff-0cd1265e69c7", "Seller", "SELLER" },
                    { "7d8684d8-6135-4965-b9cb-e50eaf6f02f8", "7d8684d8-6135-4965-b9cb-e50eaf6f02f8", "Admin", "ADMIN" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Manufacturers_ManufacturerId",
                table: "Products",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
