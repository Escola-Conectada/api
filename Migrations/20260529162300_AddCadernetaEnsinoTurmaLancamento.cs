using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCadernetaEnsinoTurmaLancamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProfessorUsuario",
                table: "CadernetaDigital",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoEnsino",
                table: "CadernetaDigital",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTurmaEnsino",
                table: "CadernetaDigital",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE caderneta
                SET
                    IdProfessorUsuario = disciplina.IdProfessorUsuario,
                    IdTurmaEnsino = disciplina.IdTurmaEnsino,
                    IdTipoEnsino = turma.IdTipoEnsino
                FROM CadernetaDigital AS caderneta
                INNER JOIN Disciplina AS disciplina
                    ON disciplina.IdDisciplina = caderneta.IdDisciplina
                LEFT JOIN TurmaEnsino AS turma
                    ON turma.IdTurmaEnsino = disciplina.IdTurmaEnsino
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CadernetaDigital_IdProfessorUsuario",
                table: "CadernetaDigital",
                column: "IdProfessorUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_CadernetaDigital_IdTipoEnsino",
                table: "CadernetaDigital",
                column: "IdTipoEnsino");

            migrationBuilder.CreateIndex(
                name: "IX_CadernetaDigital_IdTurmaEnsino",
                table: "CadernetaDigital",
                column: "IdTurmaEnsino");

            migrationBuilder.AddForeignKey(
                name: "FK_CadernetaDigital_TipoEnsino_IdTipoEnsino",
                table: "CadernetaDigital",
                column: "IdTipoEnsino",
                principalTable: "TipoEnsino",
                principalColumn: "IdTipoEnsino",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CadernetaDigital_TurmaEnsino_IdTurmaEnsino",
                table: "CadernetaDigital",
                column: "IdTurmaEnsino",
                principalTable: "TurmaEnsino",
                principalColumn: "IdTurmaEnsino",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CadernetaDigital_Usuario_IdProfessorUsuario",
                table: "CadernetaDigital",
                column: "IdProfessorUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CadernetaDigital_TipoEnsino_IdTipoEnsino",
                table: "CadernetaDigital");

            migrationBuilder.DropForeignKey(
                name: "FK_CadernetaDigital_TurmaEnsino_IdTurmaEnsino",
                table: "CadernetaDigital");

            migrationBuilder.DropForeignKey(
                name: "FK_CadernetaDigital_Usuario_IdProfessorUsuario",
                table: "CadernetaDigital");

            migrationBuilder.DropIndex(
                name: "IX_CadernetaDigital_IdProfessorUsuario",
                table: "CadernetaDigital");

            migrationBuilder.DropIndex(
                name: "IX_CadernetaDigital_IdTipoEnsino",
                table: "CadernetaDigital");

            migrationBuilder.DropIndex(
                name: "IX_CadernetaDigital_IdTurmaEnsino",
                table: "CadernetaDigital");

            migrationBuilder.DropColumn(
                name: "IdProfessorUsuario",
                table: "CadernetaDigital");

            migrationBuilder.DropColumn(
                name: "IdTipoEnsino",
                table: "CadernetaDigital");

            migrationBuilder.DropColumn(
                name: "IdTurmaEnsino",
                table: "CadernetaDigital");
        }
    }
}
