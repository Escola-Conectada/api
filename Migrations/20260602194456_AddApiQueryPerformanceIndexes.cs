using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddApiQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notificacao_IdUsuario_CriadaEmUtc",
                table: "Notificacao",
                columns: new[] { "IdUsuario", "CriadaEmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacao_IdUsuario_Lida",
                table: "Notificacao",
                columns: new[] { "IdUsuario", "Lida" });

            migrationBuilder.CreateIndex(
                name: "IX_CadernetaDigital_IdAlunoUsuario_IdTurmaEnsino_IdDisciplina",
                table: "CadernetaDigital",
                columns: new[] { "IdAlunoUsuario", "IdTurmaEnsino", "IdDisciplina" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notificacao_IdUsuario_CriadaEmUtc",
                table: "Notificacao");

            migrationBuilder.DropIndex(
                name: "IX_Notificacao_IdUsuario_Lida",
                table: "Notificacao");

            migrationBuilder.DropIndex(
                name: "IX_CadernetaDigital_IdAlunoUsuario_IdTurmaEnsino_IdDisciplina",
                table: "CadernetaDigital");
        }
    }
}
