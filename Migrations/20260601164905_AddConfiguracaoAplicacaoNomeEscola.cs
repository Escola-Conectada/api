using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracaoAplicacaoNomeEscola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracaoAplicacao",
                columns: table => new
                {
                    IdConfiguracaoAplicacao = table.Column<int>(type: "int", nullable: false),
                    NomeEscola = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracaoAplicacao", x => x.IdConfiguracaoAplicacao);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracaoAplicacao",
                columns: new[] { "IdConfiguracaoAplicacao", "AtualizadoEmUtc", "NomeEscola" },
                values: new object[] { 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Escola Conectada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracaoAplicacao");
        }
    }
}
