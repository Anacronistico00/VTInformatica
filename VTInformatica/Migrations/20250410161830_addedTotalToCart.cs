using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class addedTotalToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bf090f13-1b4e-4fa2-8d04-9671fa60dccc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cc0babd8-79e4-43a0-9338-4e2ecfc1654f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ffb80785-c550-4f27-9a80-d7b625731f41");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Carts");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "bf090f13-1b4e-4fa2-8d04-9671fa60dccc", "bf090f13-1b4e-4fa2-8d04-9671fa60dccc", "Customer", "CUSTOMER" },
                    { "cc0babd8-79e4-43a0-9338-4e2ecfc1654f", "cc0babd8-79e4-43a0-9338-4e2ecfc1654f", "Seller", "SELLER" },
                    { "ffb80785-c550-4f27-9a80-d7b625731f41", "ffb80785-c550-4f27-9a80-d7b625731f41", "Admin", "ADMIN" }
                });
        }
    }
}
