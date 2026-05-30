using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAlunoTurmaEnsinoMatriculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlunoTurmaEnsino",
                columns: table => new
                {
                    IdAlunoTurmaEnsino = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlunoUsuario = table.Column<int>(type: "int", nullable: false),
                    IdTurmaEnsino = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioResponsavel = table.Column<int>(type: "int", nullable: true),
                    MatriculadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunoTurmaEnsino", x => x.IdAlunoTurmaEnsino);
                    table.ForeignKey(
                        name: "FK_AlunoTurmaEnsino_TurmaEnsino_IdTurmaEnsino",
                        column: x => x.IdTurmaEnsino,
                        principalTable: "TurmaEnsino",
                        principalColumn: "IdTurmaEnsino",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlunoTurmaEnsino_Usuario_IdAlunoUsuario",
                        column: x => x.IdAlunoUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlunoTurmaEnsino_Usuario_IdUsuarioResponsavel",
                        column: x => x.IdUsuarioResponsavel,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO AlunoTurmaEnsino (IdAlunoUsuario, IdTurmaEnsino, IdUsuarioResponsavel, MatriculadoEmUtc)
                SELECT
                    caderneta.IdAlunoUsuario,
                    MIN(caderneta.IdTurmaEnsino),
                    MIN(caderneta.IdProfessorUsuario),
                    SYSUTCDATETIME()
                FROM CadernetaDigital AS caderneta
                INNER JOIN Usuario AS aluno
                    ON aluno.IdUsuario = caderneta.IdAlunoUsuario
                    AND aluno.IdPerfil = 3
                WHERE caderneta.IdTurmaEnsino IS NOT NULL
                GROUP BY caderneta.IdAlunoUsuario
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AlunoTurmaEnsino_IdAlunoUsuario",
                table: "AlunoTurmaEnsino",
                column: "IdAlunoUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlunoTurmaEnsino_IdTurmaEnsino",
                table: "AlunoTurmaEnsino",
                column: "IdTurmaEnsino");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoTurmaEnsino_IdUsuarioResponsavel",
                table: "AlunoTurmaEnsino",
                column: "IdUsuarioResponsavel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlunoTurmaEnsino");
        }
    }
}
