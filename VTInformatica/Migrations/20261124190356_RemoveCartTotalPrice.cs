using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCartTotalPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Carts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: true);

            // Il totale torna persistito: lo si ripopola dalle righe e dai prezzi correnti.
            migrationBuilder.Sql(
                @"UPDATE c SET c.TotalPrice = ISNULL((
                    SELECT SUM(ci.Quantity * p.Price)
                    FROM CartItems ci INNER JOIN Products p ON p.Id = ci.ProductId
                    WHERE ci.CartId = c.Id), 0)
                  FROM Carts c");
        }
    }
}
