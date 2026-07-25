using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalgaFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PixManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PagamentoConfirmadoEm",
                table: "Pedidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusPagamento",
                table: "Pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PixAtivo",
                table: "Empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PixChave",
                table: "Empresas",
                type: "character varying(140)",
                maxLength: 140,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixNomeBeneficiario",
                table: "Empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagamentoConfirmadoEm",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "StatusPagamento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "PixAtivo",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "PixChave",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "PixNomeBeneficiario",
                table: "Empresas");
        }
    }
}
