using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddBoletimDigital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoletimDigital",
                columns: table => new
                {
                    IdBoletimDigital = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlunoUsuario = table.Column<int>(type: "int", nullable: false),
                    IdTurmaEnsino = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdProfessorSolicitanteUsuario = table.Column<int>(type: "int", nullable: true),
                    SolicitadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdAdministradorLiberacaoUsuario = table.Column<int>(type: "int", nullable: true),
                    LiberadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoletimDigital", x => x.IdBoletimDigital);
                    table.ForeignKey(
                        name: "FK_BoletimDigital_TurmaEnsino_IdTurmaEnsino",
                        column: x => x.IdTurmaEnsino,
                        principalTable: "TurmaEnsino",
                        principalColumn: "IdTurmaEnsino",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoletimDigital_Usuario_IdAdministradorLiberacaoUsuario",
                        column: x => x.IdAdministradorLiberacaoUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoletimDigital_Usuario_IdAlunoUsuario",
                        column: x => x.IdAlunoUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoletimDigital_Usuario_IdProfessorSolicitanteUsuario",
                        column: x => x.IdProfessorSolicitanteUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoletimDigital_IdAdministradorLiberacaoUsuario",
                table: "BoletimDigital",
                column: "IdAdministradorLiberacaoUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_BoletimDigital_IdAlunoUsuario_IdTurmaEnsino",
                table: "BoletimDigital",
                columns: new[] { "IdAlunoUsuario", "IdTurmaEnsino" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoletimDigital_IdProfessorSolicitanteUsuario",
                table: "BoletimDigital",
                column: "IdProfessorSolicitanteUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_BoletimDigital_IdTurmaEnsino",
                table: "BoletimDigital",
                column: "IdTurmaEnsino");

            migrationBuilder.CreateIndex(
                name: "IX_BoletimDigital_Status",
                table: "BoletimDigital",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoletimDigital");
        }
    }
}
