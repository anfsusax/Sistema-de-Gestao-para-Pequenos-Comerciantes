using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalgaFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PedidoFormaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nullable: pedidos criados pelo painel administrativo (fora do cardápio público)
            // não são obrigados a informar forma de pagamento — a exigência é só no checkout
            // público (validada em código, LojaPublicaService.CriarPedidoVisitanteAsync).
            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "Pedidos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Pedidos");
        }
    }
}
