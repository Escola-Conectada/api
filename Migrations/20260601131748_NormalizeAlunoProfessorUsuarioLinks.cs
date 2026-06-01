using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAlunoProfessorUsuarioLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Usuario_IdUsuario",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Usuario_IdUsuario",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Professores_IdUsuario",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos");

            migrationBuilder.Sql(
                """
                DELETE aluno
                FROM Alunos AS aluno
                WHERE aluno.IdUsuario IS NULL
                   OR NOT EXISTS (
                        SELECT 1
                        FROM Usuario AS usuario
                        WHERE usuario.IdUsuario = aluno.IdUsuario
                          AND usuario.IdPerfil = 3
                   )
                   OR NOT EXISTS (
                        SELECT 1
                        FROM Professores AS professor
                        INNER JOIN Usuario AS usuarioProfessor
                            ON usuarioProfessor.IdUsuario = professor.IdUsuario
                           AND usuarioProfessor.IdPerfil = 2
                        WHERE professor.Id = aluno.ProfessorId
                   );

                WITH AlunosDuplicados AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY IdUsuario ORDER BY Id) AS OrdemDuplicidade
                    FROM Alunos
                )
                DELETE FROM AlunosDuplicados
                WHERE OrdemDuplicidade > 1;

                DELETE professor
                FROM Professores AS professor
                WHERE professor.IdUsuario IS NULL
                   OR NOT EXISTS (
                        SELECT 1
                        FROM Usuario AS usuario
                        WHERE usuario.IdUsuario = professor.IdUsuario
                          AND usuario.IdPerfil = 2
                   );

                WITH ProfessoresDuplicados AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY IdUsuario ORDER BY Id) AS OrdemDuplicidade
                    FROM Professores
                )
                DELETE FROM ProfessoresDuplicados
                WHERE OrdemDuplicidade > 1;
                """);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Professores",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "Professores",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "Alunos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Professores_IdUsuario",
                table: "Professores",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Usuario_IdUsuario",
                table: "Alunos",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Professores_Usuario_IdUsuario",
                table: "Professores",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Usuario_IdUsuario",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_Professores_Usuario_IdUsuario",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Professores_IdUsuario",
                table: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos");

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "Professores",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "Alunos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "DataNasc", "IdUsuario", "Nome", "ProfessorId", "Sobrenome" },
                values: new object[] { 10, "10/10/1989", null, "Gabriela", 10, "Rodrigues" });

            migrationBuilder.InsertData(
                table: "Professores",
                columns: new[] { "Id", "IdUsuario", "Nome" },
                values: new object[,]
                {
                    { 11, null, "Aline" },
                    { 12, null, "Eduardo" },
                    { 13, null, "Juliana" },
                    { 14, null, "Renato" },
                    { 15, null, "Camila" },
                    { 16, null, "Gustavo" },
                    { 17, null, "Beatriz" },
                    { 18, null, "Felipe" },
                    { 19, null, "Larissa" },
                    { 20, null, "Diego" },
                    { 21, null, "Tatiane" },
                    { 22, null, "Rafael" },
                    { 23, null, "Carolina" },
                    { 24, null, "Henrique" },
                    { 25, null, "Vanessa" },
                    { 26, null, "Leonardo" },
                    { 27, null, "Priscila" },
                    { 28, null, "Andre" },
                    { 29, null, "Simone" },
                    { 30, null, "Thiago" },
                    { 31, null, "Monica" },
                    { 32, null, "Fabio" },
                    { 33, null, "Daniela" },
                    { 34, null, "Rodrigo" },
                    { 35, null, "Leticia" },
                    { 36, null, "Sergio" },
                    { 37, null, "Bruna" },
                    { 38, null, "Caio" },
                    { 39, null, "Gabriela" },
                    { 40, null, "Samuel" },
                    { 41, null, "Isabela" },
                    { 42, null, "Lucas" },
                    { 43, null, "Natalia" },
                    { 44, null, "Paulo" },
                    { 45, null, "Bianca" },
                    { 46, null, "Matheus" },
                    { 47, null, "Renata" },
                    { 48, null, "Vitor" },
                    { 49, null, "Amanda" },
                    { 50, null, "Leandro" }
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "DataNasc", "IdUsuario", "Nome", "ProfessorId", "Sobrenome" },
                values: new object[,]
                {
                    { 11, "11/11/1990", null, "Hugo", 11, "Almeida" },
                    { 12, "12/12/1991", null, "Isabela", 12, "Nascimento" },
                    { 13, "13/01/1992", null, "Jonas", 13, "Lima" },
                    { 14, "14/02/1993", null, "Karina", 14, "Araujo" },
                    { 15, "15/03/1994", null, "Luis", 15, "Ferreira" },
                    { 16, "16/04/1995", null, "Manuela", 16, "Carvalho" },
                    { 17, "17/05/1996", null, "Nicolas", 17, "Ribeiro" },
                    { 18, "18/06/1997", null, "Olivia", 18, "Martins" },
                    { 19, "19/07/1998", null, "Pedro", 19, "Rocha" },
                    { 20, "20/08/1999", null, "Rafaela", 20, "Barbosa" },
                    { 21, "21/09/2000", null, "Sofia", 21, "Dias" },
                    { 22, "22/10/2001", null, "Tiago", 22, "Teixeira" },
                    { 23, "23/11/2002", null, "Ursula", 23, "Correia" },
                    { 24, "24/12/2003", null, "Victor", 24, "Mendes" },
                    { 25, "25/01/2004", null, "Wesley", 25, "Cardoso" },
                    { 26, "26/02/1980", null, "Yasmin", 26, "Ramos" },
                    { 27, "27/03/1981", null, "Zoe", 27, "Castro" },
                    { 28, "28/04/1982", null, "Arthur", 28, "Fernandes" },
                    { 29, "01/05/1983", null, "Bianca", 29, "Moreira" },
                    { 30, "02/06/1984", null, "Caio", 30, "Moura" },
                    { 31, "03/07/1985", null, "Debora", 31, "Batista" },
                    { 32, "04/08/1986", null, "Enzo", 32, "Freitas" },
                    { 33, "05/09/1987", null, "Flavia", 33, "Monteiro" },
                    { 34, "06/10/1988", null, "Guilherme", 34, "Campos" },
                    { 35, "07/11/1989", null, "Helena", 35, "Vieira" },
                    { 36, "08/12/1990", null, "Igor", 36, "Pinto" },
                    { 37, "09/01/1991", null, "Julia", 37, "Cavalcanti" },
                    { 38, "10/02/1992", null, "Kevin", 38, "Farias" },
                    { 39, "11/03/1993", null, "Laura", 39, "Cunha" },
                    { 40, "12/04/1994", null, "Miguel", 40, "Duarte" },
                    { 41, "13/05/1995", null, "Natalia", 41, "Lopes" },
                    { 42, "14/06/1996", null, "Otavio", 42, "Reis" },
                    { 43, "15/07/1997", null, "Pamela", 43, "Pires" },
                    { 44, "16/08/1998", null, "Rafael", 44, "Tavares" },
                    { 45, "17/09/1999", null, "Sabrina", 45, "Mello" },
                    { 46, "18/10/2000", null, "Tales", 46, "Assis" },
                    { 47, "19/11/2001", null, "Vanessa", 47, "Peixoto" },
                    { 48, "20/12/2002", null, "William", 48, "Nunes" },
                    { 49, "21/01/2003", null, "Xenia", 49, "Macedo" },
                    { 50, "22/02/2004", null, "Yuri", 50, "Brito" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Professores_IdUsuario",
                table: "Professores",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Usuario_IdUsuario",
                table: "Alunos",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Professores_Usuario_IdUsuario",
                table: "Professores",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
