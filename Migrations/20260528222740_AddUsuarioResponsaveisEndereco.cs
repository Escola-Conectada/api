using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioResponsaveisEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Usuario",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeMae",
                table: "Usuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomePai",
                table: "Usuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "NomeMae",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "NomePai",
                table: "Usuario");
        }
    }
}
