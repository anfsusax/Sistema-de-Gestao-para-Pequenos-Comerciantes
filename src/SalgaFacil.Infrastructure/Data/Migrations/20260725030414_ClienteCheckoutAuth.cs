using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalgaFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClienteCheckoutAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "Clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "Clientes");
        }
    }
}
