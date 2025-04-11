using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class modifiedGetByIdToEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "41d814da-c9f5-4b36-b79f-d1ce0ee4ca46");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "495c860c-6c2d-4abc-af69-9216ef05e6ae");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4b9a6237-73a3-4ca4-afec-1043c1705892");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Carts");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "41d814da-c9f5-4b36-b79f-d1ce0ee4ca46", "41d814da-c9f5-4b36-b79f-d1ce0ee4ca46", "Admin", "ADMIN" },
                    { "495c860c-6c2d-4abc-af69-9216ef05e6ae", "495c860c-6c2d-4abc-af69-9216ef05e6ae", "Seller", "SELLER" },
                    { "4b9a6237-73a3-4ca4-afec-1043c1705892", "4b9a6237-73a3-4ca4-afec-1043c1705892", "Customer", "CUSTOMER" }
                });
        }
    }
}
