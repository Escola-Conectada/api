using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddDadosNotasNotificacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTipoEnsino",
                table: "Notificacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTurmaEnsino",
                table: "Notificacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeTipoEnsino",
                table: "Notificacao",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeTurmaEnsino",
                table: "Notificacao",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Notificacao",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTipoEnsino",
                table: "Notificacao");

            migrationBuilder.DropColumn(
                name: "IdTurmaEnsino",
                table: "Notificacao");

            migrationBuilder.DropColumn(
                name: "NomeTipoEnsino",
                table: "Notificacao");

            migrationBuilder.DropColumn(
                name: "NomeTurmaEnsino",
                table: "Notificacao");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Notificacao");
        }
    }
}
