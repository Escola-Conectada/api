using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioDataNascimentoDisciplinaEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento",
                table: "Usuario",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DisciplinaEvento",
                columns: table => new
                {
                    IdEventoDisciplina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDisciplina = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisciplinaEvento", x => x.IdEventoDisciplina);
                    table.ForeignKey(
                        name: "FK_DisciplinaEvento_Disciplina_IdDisciplina",
                        column: x => x.IdDisciplina,
                        principalTable: "Disciplina",
                        principalColumn: "IdDisciplina",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 3,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 4,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 5,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 6,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 7,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 8,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 9,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 11,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 12,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 13,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 14,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 15,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 16,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 17,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 18,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 19,
                column: "DataNascimento",
                value: null);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 20,
                column: "DataNascimento",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_DisciplinaEvento_IdDisciplina_Data",
                table: "DisciplinaEvento",
                columns: new[] { "IdDisciplina", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisciplinaEvento");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Usuario");
        }
    }
}
