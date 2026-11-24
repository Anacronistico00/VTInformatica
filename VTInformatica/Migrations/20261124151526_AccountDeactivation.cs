using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VTInformatica.Migrations
{
    /// <inheritdoc />
    public partial class AccountDeactivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeactivationNote",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeactivationReason",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            // Backfill: finora l'unico modo di disattivare era l'Admin (DeleteUserByEmail),
            // quindi gli utenti già disattivati diventano Reason=Admin (2). Data e nota restano NULL.
            migrationBuilder.Sql("UPDATE [AspNetUsers] SET [DeactivationReason] = 2 WHERE [IsActive] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeactivationNote",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "AspNetUsers");
        }
    }
}
