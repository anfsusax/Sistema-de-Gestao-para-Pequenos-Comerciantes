using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalgaFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComprovantePixManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComprovanteCaminho",
                table: "Pedidos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteNomeOriginal",
                table: "Pedidos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteContentType",
                table: "Pedidos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ComprovanteTamanhoBytes",
                table: "Pedidos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComprovanteEnviadoEm",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteMotivoRejeicao",
                table: "Pedidos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComprovanteRevisadoEm",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos",
                column: "ComprovanteRevisadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos",
                column: "PagamentoConfirmadoPorUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos",
                column: "ComprovanteRevisadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos",
                column: "PagamentoConfirmadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteCaminho",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteNomeOriginal",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteContentType",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteTamanhoBytes",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteEnviadoEm",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteMotivoRejeicao",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteRevisadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ComprovanteRevisadoEm",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "PagamentoConfirmadoPorUsuarioId",
                table: "Pedidos");
        }
    }
}
