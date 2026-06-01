using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedFiveAlunosPerTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @AlunoIds TABLE (IdUsuario int PRIMARY KEY);

                INSERT INTO @AlunoIds (IdUsuario)
                SELECT IdUsuario
                FROM Usuario
                WHERE IdPerfil = 3;

                DELETE FROM BoletimDigital
                WHERE IdAlunoUsuario IN (SELECT IdUsuario FROM @AlunoIds)
                   OR IdProfessorSolicitanteUsuario IN (SELECT IdUsuario FROM @AlunoIds)
                   OR IdAdministradorLiberacaoUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Notificacao
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM CadernetaDigital
                WHERE IdAlunoUsuario IN (SELECT IdUsuario FROM @AlunoIds)
                   OR IdProfessorUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM AlunoTurmaEnsino
                WHERE IdAlunoUsuario IN (SELECT IdUsuario FROM @AlunoIds)
                   OR IdUsuarioResponsavel IN (SELECT IdUsuario FROM @AlunoIds);

                UPDATE Diretoria
                SET IdUsuario = NULL
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                UPDATE Disciplina
                SET IdProfessorUsuario = NULL
                WHERE IdProfessorUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Arquivo
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Holerite
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                UPDATE CalendarioEscolarEvento
                SET IdUsuarioCriador = NULL
                WHERE IdUsuarioCriador IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Alunos
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Professores
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Usuario
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);
                """);

            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "IdUsuario", "DataNascimento", "Email", "Endereco", "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "FotoPerfilUrl", "IdPerfil", "IdUsuarioCriador", "Nome", "NomeMae", "NomePai", "NomeUsuarioCriador", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash", "Senha", "Telefone" },
                values: new object[,]
                {
                    { 12, null, "aluno01@escola.com", null, null, null, null, 3, null, "Aluno Maria", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEy$PSoiqNNWzRiAvRVzute150mhDSL4rTOTisKBi9TTrJM=", "11977770001" },
                    { 13, null, "aluno02@escola.com", null, null, null, null, 3, null, "Aluno Joao", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEz$sxGpNfM7WC8nMjkLmcQ9WN2J6Lhe7O07oyFXwLO4/iU=", "11977770002" },
                    { 14, null, "aluno03@escola.com", null, null, null, null, 3, null, "Aluno Alex", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE0$0mh8FyroAgQNjTYZXcypWMYARFKyzXJ48MNVyB78E+U=", "11977770003" },
                    { 15, null, "aluno04@escola.com", null, null, null, null, 3, null, "Aluno Ana", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE1$SpupVdnneUNyvnPwlmVBvm0OUOr/Yjov1o5xA20Q1+w=", "11977770004" },
                    { 16, null, "aluno05@escola.com", null, null, null, null, 3, null, "Aluno Bruno", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE2$oqDVRdKBe7SS3kMj3hAd6kgoC9dH9f8fyOHlTOZrl9k=", "11977770005" },
                    { 17, null, "aluno06@escola.com", null, null, null, null, 3, null, "Aluno Carla", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE3$ss/qC9hAEFzI4JwGoPvKuAuI4DN8bXtKp1Ulbt/WF1o=", "11977770006" },
                    { 18, null, "aluno07@escola.com", null, null, null, null, 3, null, "Aluno Daniel", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE4$Ks5ex2/VDVyP7B3EbbqTJqr/34e1PGXqDCTx30kSYdk=", "11977770007" },
                    { 19, null, "aluno08@escola.com", null, null, null, null, 3, null, "Aluno Elisa", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTE5$p4DpMKEz6wYRtFN4Qimq7IhXnIl+So6O8D7Zl82D59M=", "11977770008" },
                    { 20, null, "aluno09@escola.com", null, null, null, null, 3, null, "Aluno Fabio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTIw$XIIyE2s98F4W58nJ2sbRe5rriyLwskk2/VxBAgNNSZc=", "11977770009" }
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "DataNasc", "IdUsuario", "Nome", "ProfessorId", "Sobrenome" },
                values: new object[,]
                {
                    { 1, "25/02/1982", 12, "Maria", 1, "Solano" },
                    { 2, "25/01/2000", 13, "Joao", 2, "Gomes" },
                    { 3, "22/02/2002", 14, "Alex", 3, "Alves" },
                    { 4, "04/04/2008", 15, "Ana", 4, "Silva" },
                    { 5, "05/05/2009", 16, "Bruno", 5, "Santos" },
                    { 6, "06/06/2010", 17, "Carla", 6, "Oliveira" },
                    { 7, "07/07/2011", 18, "Daniel", 7, "Souza" },
                    { 8, "08/08/2012", 19, "Elisa", 8, "Pereira" },
                    { 9, "09/09/2013", 20, "Fabio", 9, "Costa" }
                });

            migrationBuilder.InsertData(
                table: "AlunoTurmaEnsino",
                columns: new[] { "IdAlunoTurmaEnsino", "IdAlunoUsuario", "IdTurmaEnsino", "IdUsuarioResponsavel", "MatriculadoEmUtc" },
                values: new object[,]
                {
                    { 10001, 12, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10002, 13, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10003, 14, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10004, 15, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10005, 16, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10006, 17, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10007, 18, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10008, 19, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10009, 20, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataNasc",
                value: "04/04/2008");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataNasc",
                value: "05/05/2009");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataNasc",
                value: "06/06/2010");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataNasc",
                value: "07/07/2011");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataNasc",
                value: "08/08/2012");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataNasc",
                value: "09/09/2013");

            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "IdUsuario", "DataNascimento", "Email", "Endereco", "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "FotoPerfilUrl", "IdPerfil", "IdUsuarioCriador", "Nome", "NomeMae", "NomePai", "NomeUsuarioCriador", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash", "Senha", "Telefone" },
                values: new object[,]
                {
                    { 10001, null, "aluno10@escola.com", null, null, null, null, 3, null, "Aluno Gabriela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDAx$9jsC1UppnavgKi4KwRJNMZcgv37y0BwlWxjkpBK3f8k=", "11977770010" },
                    { 10002, null, "aluno11@escola.com", null, null, null, null, 3, null, "Aluno Hugo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDAy$rLHEn6V2gBeHtxoxhEwmT5HdgOuYK4iMHL+rZhDPGbc=", "11977770011" },
                    { 10003, null, "aluno12@escola.com", null, null, null, null, 3, null, "Aluno Isabela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDAz$qWmuSS6fXfAKlDMTOofWR7k5pmduVmCBrg/SWv/8m4A=", "11977770012" },
                    { 10004, null, "aluno13@escola.com", null, null, null, null, 3, null, "Aluno Jonas", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA0$y019sQfhg2EWIAO1YI4e8Q3PL/CayDr2WoAlz2Ms3TY=", "11977770013" },
                    { 10005, null, "aluno14@escola.com", null, null, null, null, 3, null, "Aluno Karina", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA1$Pko8svOQnpwU5HXBuMqFpTtYSzazhKJPVJ1RfXdmlG0=", "11977770014" },
                    { 10006, null, "aluno15@escola.com", null, null, null, null, 3, null, "Aluno Luis", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA2$MWepUwDm8k0wHFYq95g1I4sPf/XtMfSKQF2fmhrI01g=", "11977770015" },
                    { 10007, null, "aluno16@escola.com", null, null, null, null, 3, null, "Aluno Manuela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA3$HRsuFuhGK6V9QdWDY5dV1dLzYHaI5wAt46j15QcU4Yg=", "11977770016" },
                    { 10008, null, "aluno17@escola.com", null, null, null, null, 3, null, "Aluno Nicolas", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA4$c2UdxmgCcTXIkapCJ/LqTiDXmGeTyVgwpgaaeyGmMsU=", "11977770017" },
                    { 10009, null, "aluno18@escola.com", null, null, null, null, 3, null, "Aluno Olivia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDA5$IbSrmAWW3RfnzJbvCj5ZNFSmVt4X23Gx6sGkHWO94AQ=", "11977770018" },
                    { 10010, null, "aluno19@escola.com", null, null, null, null, 3, null, "Aluno Pedro", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDEw$ujB32G9A0++rHsogRlTmmccpug54QTqhpk35bsuIcCs=", "11977770019" },
                    { 10011, null, "aluno20@escola.com", null, null, null, null, 3, null, "Aluno Rafaela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDEx$jPFAzTeE8sB4OBAhAqnGyuei1jkkkFegIVAhGh0U/rI=", "11977770020" },
                    { 10012, null, "aluno21@escola.com", null, null, null, null, 3, null, "Aluno Sofia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDEy$EglMQRDFWXK4inEv3SWiPbB0JwXyeuTOS5fRKKePxj0=", "11977770021" },
                    { 10013, null, "aluno22@escola.com", null, null, null, null, 3, null, "Aluno Tiago", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDEz$WzKRbBVQCNZ7drl76gvp4oNWVJpw3MXeqRaX6QuxwM4=", "11977770022" },
                    { 10014, null, "aluno23@escola.com", null, null, null, null, 3, null, "Aluno Ursula", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE0$0/ooRSkPp9yLUh+eItTX9dAzmUKocJUu5k8m+zJhXMk=", "11977770023" },
                    { 10015, null, "aluno24@escola.com", null, null, null, null, 3, null, "Aluno Victor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE1$TOiU/FciPzcvOlkK9zBclWDwS50dxDNEhxfBbGRJ5dw=", "11977770024" },
                    { 10016, null, "aluno25@escola.com", null, null, null, null, 3, null, "Aluno Wesley", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE2$tpzIv1/WR7p9kVtpB+6JWH6YBCarx2QOsbv8E2YqaqQ=", "11977770025" },
                    { 10017, null, "aluno26@escola.com", null, null, null, null, 3, null, "Aluno Yasmin", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE3$2Pkpw5z3wTKNO5FJ4tjIUxXeiTEsc6+wCCuNq0J8oTk=", "11977770026" },
                    { 10018, null, "aluno27@escola.com", null, null, null, null, 3, null, "Aluno Zoe", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE4$l12MPAkIcEaOsOfP+qPwwKXuhh+NenAKksdLrKOYZJQ=", "11977770027" },
                    { 10019, null, "aluno28@escola.com", null, null, null, null, 3, null, "Aluno Arthur", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDE5$EHrSp6QsCVI/8wV6/SZ8qUIJmuGxsQeC3bNTdkQ1Q/w=", "11977770028" },
                    { 10020, null, "aluno29@escola.com", null, null, null, null, 3, null, "Aluno Bianca", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDIw$3RM1ADK3BORttVTHsNJMw/YEzSqUOBR/YA2D+qpNX9o=", "11977770029" },
                    { 10021, null, "aluno30@escola.com", null, null, null, null, 3, null, "Aluno Caio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDIx$r9ZmcZyoBe15GbkpazYc1Tl11IC9TezoayY7K51eerg=", "11977770030" },
                    { 10022, null, "aluno31@escola.com", null, null, null, null, 3, null, "Aluno Debora", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDIy$a/KqlKjeQCiZ0ZwRImcXopQU/WIJZyZwMcZFMkvR95k=", "11977770031" },
                    { 10023, null, "aluno32@escola.com", null, null, null, null, 3, null, "Aluno Enzo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDIz$1R68SbmohbuKgijQ5MKNBR926XuTzKd1TeK8XctwPBQ=", "11977770032" },
                    { 10024, null, "aluno33@escola.com", null, null, null, null, 3, null, "Aluno Flavia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI0$gn5Plj1ftW1PXsAqoXD1wVK0ZnawPwWtO5N1Utde8Mo=", "11977770033" },
                    { 10025, null, "aluno34@escola.com", null, null, null, null, 3, null, "Aluno Guilherme", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI1$VKlWyN4IyVXfrZft1t1TmRQ2ytzqdHW79WNxYD5z7Wo=", "11977770034" },
                    { 10026, null, "aluno35@escola.com", null, null, null, null, 3, null, "Aluno Helena", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI2$8ggeUSSP3SpJF4JdwA6aa1ff9vbAwEcVYHXScOWGKII=", "11977770035" },
                    { 10027, null, "aluno36@escola.com", null, null, null, null, 3, null, "Aluno Igor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI3$ZcH2IlRaamy20G4sTgMGCPOji6THqLgTWexCBFbPNPE=", "11977770036" },
                    { 10028, null, "aluno37@escola.com", null, null, null, null, 3, null, "Aluno Julia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI4$6/3bep0tmdYaI3iFRa1RvKFS/4YCmplOzZNtTPyEpns=", "11977770037" },
                    { 10029, null, "aluno38@escola.com", null, null, null, null, 3, null, "Aluno Kevin", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDI5$4tLvg1J1aSRPXvViWUzoz7ujkHIncAbmaDoQnqrtYeY=", "11977770038" },
                    { 10030, null, "aluno39@escola.com", null, null, null, null, 3, null, "Aluno Laura", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDMw$ylI6I31jWcWuXkkIFbXv9U/bn8yHtDY/W7EJRfzzvH0=", "11977770039" },
                    { 10031, null, "aluno40@escola.com", null, null, null, null, 3, null, "Aluno Miguel", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDMx$l1dZz3g6EJO5VshgWBg449LOSMsK0LVRiVe/i2j2e9Y=", "11977770040" },
                    { 10032, null, "aluno41@escola.com", null, null, null, null, 3, null, "Aluno Natalia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDMy$33ECp34vCMfHlKL0ySoQvnORUPNbOFo5m2r6wVjod4g=", "11977770041" },
                    { 10033, null, "aluno42@escola.com", null, null, null, null, 3, null, "Aluno Otavio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDMz$VsI2KOkUUgiDAQfrzBiXznMh9ZSUA7ClN5m4f6O0B70=", "11977770042" },
                    { 10034, null, "aluno43@escola.com", null, null, null, null, 3, null, "Aluno Pamela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM0$ZMRlJWBsFO6TUxuhlbpmbrvHy8bLOzuszrWgE/gK6Mg=", "11977770043" },
                    { 10035, null, "aluno44@escola.com", null, null, null, null, 3, null, "Aluno Rafael", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM1$fMnfo2x89CPofCfHitrxhOXaBAJas2/NwGx6vj/d9Ug=", "11977770044" },
                    { 10036, null, "aluno45@escola.com", null, null, null, null, 3, null, "Aluno Sabrina", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM2$BPoM2ZySGRLmaTCgLNBAITYtaw3xBDNAwvCK8lPka/w=", "11977770045" },
                    { 10037, null, "aluno46@escola.com", null, null, null, null, 3, null, "Aluno Tales", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM3$YsyweSobpxUxiQkseDttedD83bwyk3uFsgE3pR7c4RU=", "11977770046" },
                    { 10038, null, "aluno47@escola.com", null, null, null, null, 3, null, "Aluno Vanessa", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM4$VxIKn6cPf4PgkrD3WOPV8yZotOC+uPp+naRWsQXOcys=", "11977770047" },
                    { 10039, null, "aluno48@escola.com", null, null, null, null, 3, null, "Aluno William", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDM5$gZ1jLugOrKgN6AauLWSTA39ep5ZJVmf0wowCl94PsKo=", "11977770048" },
                    { 10040, null, "aluno49@escola.com", null, null, null, null, 3, null, "Aluno Xenia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQw$ChuSZVqmvWrgb8c8BcSkBTMnfS5E69Av4rWo5oFFON8=", "11977770049" },
                    { 10041, null, "aluno50@escola.com", null, null, null, null, 3, null, "Aluno Yuri", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQx$RpCMQj/4YPys74cA1jQovllAlv4YVlSyItkg2t+92L8=", "11977770050" },
                    { 10042, null, "aluno51@escola.com", null, null, null, null, 3, null, "Aluno Alice", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQy$o0JWXIwoBlN0Yb+MkALBcjFz8OqLyRqL3hLefFABcCU=", "11977770051" },
                    { 10043, null, "aluno52@escola.com", null, null, null, null, 3, null, "Aluno Davi", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQz$N+fXtmUBQhXjUG96spUcVgJJBkku/EATCCVCLT7QlIE=", "11977770052" },
                    { 10044, null, "aluno53@escola.com", null, null, null, null, 3, null, "Aluno Livia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ0$BxSJg8Ad5bEUaJnWTnm4/EHZ3YHJo01EVq02VoDehaY=", "11977770053" },
                    { 10045, null, "aluno54@escola.com", null, null, null, null, 3, null, "Aluno Heitor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ1$2li24v/XCESXjFK8eV2HACyFsP4SrXsqwJl/4kDjQl0=", "11977770054" },
                    { 10046, null, "aluno55@escola.com", null, null, null, null, 3, null, "Aluno Luiza", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ2$tYYzvmdHMaKKgqCUuVzsRmgt79X7NEi+MeV3MO3FomU=", "11977770055" },
                    { 10047, null, "aluno56@escola.com", null, null, null, null, 3, null, "Aluno Theo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ3$qssKWiHU1IE/nEQ8nM1cr01LZluASFmMG6brYX35Bkw=", "11977770056" },
                    { 10048, null, "aluno57@escola.com", null, null, null, null, 3, null, "Aluno Melissa", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ4$nbPAHTQGIy0fRN6fZu06ygBPorYUoCjp5LIdARVT8A8=", "11977770057" },
                    { 10049, null, "aluno58@escola.com", null, null, null, null, 3, null, "Aluno Benicio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDQ5$k048pbARGqUYu7wU4+ffBMJyEz1J2XAzZlMf9U4hZQA=", "11977770058" },
                    { 10050, null, "aluno59@escola.com", null, null, null, null, 3, null, "Aluno Clara", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDUw$H8rxswLdAV8aAqKUgBfDB05eOQYa6BIsg1oIkXFBbVQ=", "11977770059" },
                    { 10051, null, "aluno60@escola.com", null, null, null, null, 3, null, "Aluno Isadora", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTEwMDUx$gtVSMXPz/fYWCqxsRWRDUIivAnbejeNGmo12NFi9NU8=", "11977770060" }
                });

            migrationBuilder.InsertData(
                table: "AlunoTurmaEnsino",
                columns: new[] { "IdAlunoTurmaEnsino", "IdAlunoUsuario", "IdTurmaEnsino", "IdUsuarioResponsavel", "MatriculadoEmUtc" },
                values: new object[,]
                {
                    { 10010, 10001, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10011, 10002, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10012, 10003, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10013, 10004, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10014, 10005, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10015, 10006, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10016, 10007, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10017, 10008, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10018, 10009, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10019, 10010, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10020, 10011, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10021, 10012, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10022, 10013, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10023, 10014, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10024, 10015, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10025, 10016, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10026, 10017, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10027, 10018, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10028, 10019, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10029, 10020, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10030, 10021, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10031, 10022, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10032, 10023, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10033, 10024, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10034, 10025, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10035, 10026, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10036, 10027, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10037, 10028, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10038, 10029, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10039, 10030, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10040, 10031, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10041, 10032, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10042, 10033, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10043, 10034, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10044, 10035, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10045, 10036, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10046, 10037, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10047, 10038, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10048, 10039, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10049, 10040, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10050, 10041, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10051, 10042, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10052, 10043, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10053, 10044, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10054, 10045, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10055, 10046, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10056, 10047, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10057, 10048, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10058, 10049, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10059, 10050, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10060, 10051, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "DataNasc", "IdUsuario", "Nome", "ProfessorId", "Sobrenome" },
                values: new object[,]
                {
                    { 10001, "10/10/2014", 10001, "Gabriela", 10, "Rodrigues" },
                    { 10002, "11/11/2015", 10002, "Hugo", 1, "Almeida" },
                    { 10003, "12/12/2016", 10003, "Isabela", 2, "Nascimento" },
                    { 10004, "13/01/2005", 10004, "Jonas", 3, "Lima" },
                    { 10005, "14/02/2006", 10005, "Karina", 4, "Araujo" },
                    { 10006, "15/03/2007", 10006, "Luis", 5, "Ferreira" },
                    { 10007, "16/04/2008", 10007, "Manuela", 6, "Carvalho" },
                    { 10008, "17/05/2009", 10008, "Nicolas", 7, "Ribeiro" },
                    { 10009, "18/06/2010", 10009, "Olivia", 8, "Martins" },
                    { 10010, "19/07/2011", 10010, "Pedro", 9, "Rocha" },
                    { 10011, "20/08/2012", 10011, "Rafaela", 10, "Barbosa" },
                    { 10012, "21/09/2013", 10012, "Sofia", 1, "Dias" },
                    { 10013, "22/10/2014", 10013, "Tiago", 2, "Teixeira" },
                    { 10014, "23/11/2015", 10014, "Ursula", 3, "Correia" },
                    { 10015, "24/12/2016", 10015, "Victor", 4, "Mendes" },
                    { 10016, "25/01/2005", 10016, "Wesley", 5, "Cardoso" },
                    { 10017, "26/02/2006", 10017, "Yasmin", 6, "Ramos" },
                    { 10018, "27/03/2007", 10018, "Zoe", 7, "Castro" },
                    { 10019, "28/04/2008", 10019, "Arthur", 8, "Fernandes" },
                    { 10020, "01/05/2009", 10020, "Bianca", 9, "Moreira" },
                    { 10021, "02/06/2010", 10021, "Caio", 10, "Moura" },
                    { 10022, "03/07/2011", 10022, "Debora", 1, "Batista" },
                    { 10023, "04/08/2012", 10023, "Enzo", 2, "Freitas" },
                    { 10024, "05/09/2013", 10024, "Flavia", 3, "Monteiro" },
                    { 10025, "06/10/2014", 10025, "Guilherme", 4, "Campos" },
                    { 10026, "07/11/2015", 10026, "Helena", 5, "Vieira" },
                    { 10027, "08/12/2016", 10027, "Igor", 6, "Pinto" },
                    { 10028, "09/01/2005", 10028, "Julia", 7, "Cavalcanti" },
                    { 10029, "10/02/2006", 10029, "Kevin", 8, "Farias" },
                    { 10030, "11/03/2007", 10030, "Laura", 9, "Cunha" },
                    { 10031, "12/04/2008", 10031, "Miguel", 10, "Duarte" },
                    { 10032, "13/05/2009", 10032, "Natalia", 1, "Lopes" },
                    { 10033, "14/06/2010", 10033, "Otavio", 2, "Reis" },
                    { 10034, "15/07/2011", 10034, "Pamela", 3, "Pires" },
                    { 10035, "16/08/2012", 10035, "Rafael", 4, "Tavares" },
                    { 10036, "17/09/2013", 10036, "Sabrina", 5, "Mello" },
                    { 10037, "18/10/2014", 10037, "Tales", 6, "Assis" },
                    { 10038, "19/11/2015", 10038, "Vanessa", 7, "Peixoto" },
                    { 10039, "20/12/2016", 10039, "William", 8, "Nunes" },
                    { 10040, "21/01/2005", 10040, "Xenia", 9, "Macedo" },
                    { 10041, "22/02/2006", 10041, "Yuri", 10, "Brito" },
                    { 10042, "23/03/2007", 10042, "Alice", 1, "Medeiros" },
                    { 10043, "24/04/2008", 10043, "Davi", 2, "Sales" },
                    { 10044, "25/05/2009", 10044, "Livia", 3, "Amaral" },
                    { 10045, "26/06/2010", 10045, "Heitor", 4, "Queiroz" },
                    { 10046, "27/07/2011", 10046, "Luiza", 5, "Rezende" },
                    { 10047, "28/08/2012", 10047, "Theo", 6, "Aguiar" },
                    { 10048, "01/09/2013", 10048, "Melissa", 7, "Moraes" },
                    { 10049, "02/10/2014", 10049, "Benicio", 8, "Dantas" },
                    { 10050, "03/11/2015", 10050, "Clara", 9, "Borges" },
                    { 10051, "04/12/2016", 10051, "Isadora", 10, "Neves" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10001);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10002);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10003);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10004);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10005);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10006);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10007);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10008);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10009);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10010);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10011);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10012);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10013);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10014);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10015);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10016);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10017);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10018);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10019);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10020);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10021);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10022);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10023);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10024);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10025);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10026);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10027);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10028);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10029);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10030);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10031);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10032);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10033);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10034);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10035);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10036);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10037);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10038);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10039);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10040);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10041);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10042);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10043);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10044);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10045);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10046);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10047);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10048);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10049);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10050);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10051);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10052);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10053);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10054);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10055);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10056);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10057);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10058);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10059);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10060);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10001);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10002);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10003);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10004);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10005);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10006);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10007);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10008);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10009);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10010);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10011);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10012);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10013);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10014);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10015);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10016);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10017);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10018);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10019);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10020);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10021);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10022);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10023);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10024);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10025);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10026);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10027);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10028);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10029);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10030);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10031);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10032);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10033);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10034);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10035);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10036);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10037);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10038);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10039);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10040);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10041);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10042);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10043);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10044);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10045);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10046);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10047);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10048);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10049);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10050);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 10051);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10001);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10002);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10003);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10004);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10005);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10006);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10007);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10008);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10009);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10010);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10011);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10012);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10013);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10014);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10015);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10016);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10017);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10018);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10019);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10020);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10021);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10022);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10023);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10024);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10025);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10026);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10027);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10028);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10029);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10030);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10031);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10032);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10033);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10034);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10035);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10036);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10037);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10038);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10039);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10040);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10041);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10042);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10043);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10044);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10045);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10046);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10047);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10048);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10049);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10050);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10051);

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataNasc",
                value: "04/04/1983");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataNasc",
                value: "05/05/1984");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataNasc",
                value: "06/06/1985");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataNasc",
                value: "07/07/1986");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataNasc",
                value: "08/08/1987");

            migrationBuilder.UpdateData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataNasc",
                value: "09/09/1988");
        }
    }
}
