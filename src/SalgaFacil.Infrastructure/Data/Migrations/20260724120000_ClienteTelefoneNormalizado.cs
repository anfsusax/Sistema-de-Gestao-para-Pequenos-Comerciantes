using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalgaFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClienteTelefoneNormalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Coluna nasce nula de propósito: cadastros existentes (anteriores a esta
            // funcionalidade) precisam ser normalizados por código — ver
            // ClienteManutencaoService.NormalizarTelefonesExistentesAsync (reaproveita a
            // mesma regra de TelefoneNormalizador usada em toda a aplicação, evitando
            // duplicar a lógica de normalização em SQL). Rodar essa rotina (via
            // /clientes/duplicados) antes de considerar promover este índice para UNIQUE.
            migrationBuilder.AddColumn<string>(
                name: "TelefoneNormalizado",
                table: "Clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Índice NÃO único por enquanto — ver nota acima e _ia/RISCOS.md
            // ("Restrição UNIQUE de telefone pendente de checagem de duplicados").
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId_TelefoneNormalizado",
                table: "Clientes",
                columns: new[] { "EmpresaId", "TelefoneNormalizado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_TelefoneNormalizado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TelefoneNormalizado",
                table: "Clientes");
        }
    }
}
