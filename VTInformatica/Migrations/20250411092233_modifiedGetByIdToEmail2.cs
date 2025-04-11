using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class modifiedGetByIdToEmail2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0f7a4649-ad09-411f-a879-2deb21e60660");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3d256867-27c7-4d0c-9462-73da5db4cb3e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4e61cd15-eb56-4dd3-80ac-c4d59fbd1849");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0ddaa4f4-bde9-4844-8e00-9671f09fc6b8", "0ddaa4f4-bde9-4844-8e00-9671f09fc6b8", "Customer", "CUSTOMER" },
                    { "70d6b622-4270-41af-82ff-0cd1265e69c7", "70d6b622-4270-41af-82ff-0cd1265e69c7", "Seller", "SELLER" },
                    { "7d8684d8-6135-4965-b9cb-e50eaf6f02f8", "7d8684d8-6135-4965-b9cb-e50eaf6f02f8", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                    { "0f7a4649-ad09-411f-a879-2deb21e60660", null, "Admin", "ADMIN" },
                    { "3d256867-27c7-4d0c-9462-73da5db4cb3e", null, "Customer", "CUSTOMER" },
                    { "4e61cd15-eb56-4dd3-80ac-c4d59fbd1849", null, "Seller", "SELLER" }
                });
        }
    }
}
