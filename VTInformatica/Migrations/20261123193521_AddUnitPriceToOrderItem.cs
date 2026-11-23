using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItem",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Backfill delle righe esistenti: il prezzo storico reale non è mai stato salvato,
            // l'unica approssimazione disponibile è il prezzo corrente del prodotto (AUDIT #6).
            migrationBuilder.Sql(
                "UPDATE oi SET oi.UnitPrice = p.Price " +
                "FROM [OrderItem] oi INNER JOIN [Products] p ON p.Id = oi.ProductId;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderItem");
        }
    }
}
