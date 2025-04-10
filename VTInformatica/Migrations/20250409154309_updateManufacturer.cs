using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class updateManufacturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3d00012d-8a83-44a8-b82f-5b8b3c879631");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6e252143-4396-4863-9225-a8e6ec6cc0ed");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd1b25a4-687e-4ce5-b7d8-082ccd5fcfdd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "57da7ae0-b57a-4bec-8086-f3a95c3dbc1f", "57da7ae0-b57a-4bec-8086-f3a95c3dbc1f", "Customer", "CUSTOMER" },
                    { "f58f6fb3-0472-4de5-a1e1-edeb784cc0f0", "f58f6fb3-0472-4de5-a1e1-edeb784cc0f0", "Admin", "ADMIN" },
                    { "f667784a-f5b5-43b9-8c73-bf5afc77353f", "f667784a-f5b5-43b9-8c73-bf5afc77353f", "Seller", "SELLER" }
                });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 1,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/intel.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 2,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/amd-logo-1.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 3,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/nvidia.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 4,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/asus-logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 5,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/corsair-2.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 6,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/micro-star-international-logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 7,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/gigabyte-technology-logo-2008.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 8,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/3/34/Thermalright_Logo.svg", "ThermalRight" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 9,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://cdn.worldvectorlogo.com/logos/be-quiet-logo.svg", "Be quiet!" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 10,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://cdn.worldvectorlogo.com/logos/lg-electronics.svg", "LG" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 11,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://noctua.at/pub/static/version1743603791/frontend/SIT/noctua/en_US/images/logo.svg", "Noctua" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 12,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/samsung-8.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 13,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/seagate-logo-1.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 14,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/western-digital-2.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 15,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/kingston-1.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 16,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/crucial.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 17,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/evga.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 18,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/cooler-master-black-logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 19,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/thermaltake.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 20,
                column: "ManufacturerLogo",
                value: "https://cdn.worldvectorlogo.com/logos/razer-1.svg");

            migrationBuilder.InsertData(
                table: "Manufacturer",
                columns: new[] { "ManufacturerId", "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { 21, "https://cdn.worldvectorlogo.com/logos/nzxt-1.svg", "NZXT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "57da7ae0-b57a-4bec-8086-f3a95c3dbc1f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f58f6fb3-0472-4de5-a1e1-edeb784cc0f0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f667784a-f5b5-43b9-8c73-bf5afc77353f");

            migrationBuilder.DeleteData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 21);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3d00012d-8a83-44a8-b82f-5b8b3c879631", "3d00012d-8a83-44a8-b82f-5b8b3c879631", "Seller", "SELLER" },
                    { "6e252143-4396-4863-9225-a8e6ec6cc0ed", "6e252143-4396-4863-9225-a8e6ec6cc0ed", "Admin", "ADMIN" },
                    { "cd1b25a4-687e-4ce5-b7d8-082ccd5fcfdd", "cd1b25a4-687e-4ce5-b7d8-082ccd5fcfdd", "Customer", "CUSTOMER" }
                });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 1,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/6/6a/Intel_logo_(2006-2020).svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 2,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/7/7c/AMD_Logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 3,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/2/21/Nvidia_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 4,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/2/2f/AsusTek-black-logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 5,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/1/1b/Corsair.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 6,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/2/2e/Msi-Logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 7,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/0/08/Gigabyte_Technology_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 8,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/4/48/Dell_Logo.svg", "Dell" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 9,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/3/3b/HP_logo_2012.svg", "HP" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 10,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/0/0b/Lenovo_Global_Corporate_Logo.png", "Lenovo" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 11,
                columns: new[] { "ManufacturerLogo", "ManufacturerName" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/f/fa/Apple_logo_black.svg", "Apple" });

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 12,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/2/24/Samsung_Logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 13,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/7/79/Seagate_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 14,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/2/20/Western_Digital_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 15,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/4/4a/Kingston_Technology_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 16,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/0/0f/Crucial_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 17,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/3/3c/EVGA_Logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 18,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/5/5e/Cooler_Master_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 19,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/3/3a/Thermaltake_logo.svg");

            migrationBuilder.UpdateData(
                table: "Manufacturer",
                keyColumn: "ManufacturerId",
                keyValue: 20,
                column: "ManufacturerLogo",
                value: "https://upload.wikimedia.org/wikipedia/commons/8/89/Razer_logo.svg");
        }
    }
}
