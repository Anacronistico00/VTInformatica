using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Ripopola la colonna dalla sottocategoria (la categoria di un prodotto è quella della sua
            // sottocategoria) prima di ricreare indice e FK: con 0 la FK fallirebbe su una tabella non vuota.
            migrationBuilder.Sql(
                "UPDATE p SET p.CategoryId = sc.CategoryId FROM Products p INNER JOIN SubCategories sc ON sc.Id = p.SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
