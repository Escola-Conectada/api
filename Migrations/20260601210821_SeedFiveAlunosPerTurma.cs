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

                DELETE FROM Arquivo
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Holerite
                WHERE IdUsuario IN (SELECT IdUsuario FROM @AlunoIds);

                UPDATE CalendarioEscolarEvento
                SET IdUsuarioCriador = NULL
                WHERE IdUsuarioCriador IN (SELECT IdUsuario FROM @AlunoIds);

                DELETE FROM Alunos
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
                    { 1, 12, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 13, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 14, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 15, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 16, 101, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 17, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 18, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 19, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 20, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
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
                    { 21, null, "aluno10@escola.com", null, null, null, null, 3, null, "Aluno Gabriela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTIx$8NKFAVl6gASN9sWH6lrGk3cGZdqoizTe+S2Y2dWi0G0=", "11977770010" },
                    { 22, null, "aluno11@escola.com", null, null, null, null, 3, null, "Aluno Hugo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTIy$TW92W9xDO/hEEQkLkqpRUrJBdsinS1+wHYKOaMQeyGA=", "11977770011" },
                    { 23, null, "aluno12@escola.com", null, null, null, null, 3, null, "Aluno Isabela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTIz$2lR260cX+yPHzMnl8DE5D24lAaO9fckLV+zoWGCG72A=", "11977770012" },
                    { 24, null, "aluno13@escola.com", null, null, null, null, 3, null, "Aluno Jonas", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI0$n3Q8nZv4Q68COaEefvUWVXV/GVR4xH6nEnVOPPl9tIQ=", "11977770013" },
                    { 25, null, "aluno14@escola.com", null, null, null, null, 3, null, "Aluno Karina", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI1$c6vJexJpcqvS2cU/7FSlt1prIMRGabz5gplXBp6aEsk=", "11977770014" },
                    { 26, null, "aluno15@escola.com", null, null, null, null, 3, null, "Aluno Luis", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI2$fPscQwcUSBElx6C9v6lJ3d1ElfPm8xD4VleVSDPy94o=", "11977770015" },
                    { 27, null, "aluno16@escola.com", null, null, null, null, 3, null, "Aluno Manuela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI3$c1nykWAyZw++X/8+a4XRL4KvWsxDFfmlMRq3QPDhCMU=", "11977770016" },
                    { 28, null, "aluno17@escola.com", null, null, null, null, 3, null, "Aluno Nicolas", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI4$XxiU70lyQ0grSlQeD9xT0oQ6v5c+3me5UNm8fYzRcLA=", "11977770017" },
                    { 29, null, "aluno18@escola.com", null, null, null, null, 3, null, "Aluno Olivia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTI5$f8rZU46MPoE2sKBttNNZJf2MuBtxVIUyj2R528CwUuQ=", "11977770018" },
                    { 30, null, "aluno19@escola.com", null, null, null, null, 3, null, "Aluno Pedro", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTMw$Fr3kGcoVx2O53Isb36FWa55o0DioQXr+EHXh0cWcWxU=", "11977770019" },
                    { 31, null, "aluno20@escola.com", null, null, null, null, 3, null, "Aluno Rafaela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTMx$HppyvMlTCOciXCd8+JgdAZjM2FxTa4anOQyEuf640wc=", "11977770020" },
                    { 32, null, "aluno21@escola.com", null, null, null, null, 3, null, "Aluno Sofia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTMy$P4NJRDNRCG9/C7j24Nzzn5wlNh5rW+Gkm2mwhQfuX68=", "11977770021" },
                    { 33, null, "aluno22@escola.com", null, null, null, null, 3, null, "Aluno Tiago", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTMz$aqGvLcHVRW25hxkSGeIA9yOoM8GF4j0SWQuvOTHnG4Q=", "11977770022" },
                    { 34, null, "aluno23@escola.com", null, null, null, null, 3, null, "Aluno Ursula", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM0$i+KEOx3KVyhRvTZEb440uXtHfL5wi/wDyd32LsiGH2A=", "11977770023" },
                    { 35, null, "aluno24@escola.com", null, null, null, null, 3, null, "Aluno Victor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM1$n/eU5moyS/xani66/OJrUve9ehNhqz3e/8UP5Fe/h+Y=", "11977770024" },
                    { 36, null, "aluno25@escola.com", null, null, null, null, 3, null, "Aluno Wesley", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM2$kkDXf9fWPKeUSw3lXc9CWULd193TNAuOShmAjXP59tE=", "11977770025" },
                    { 37, null, "aluno26@escola.com", null, null, null, null, 3, null, "Aluno Yasmin", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM3$6SWayH+xL68X5N8dXWlCN4GY8hmFAUA2QOv+58XYzKU=", "11977770026" },
                    { 38, null, "aluno27@escola.com", null, null, null, null, 3, null, "Aluno Zoe", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM4$ZR47XllCukrhsFg8DDEpGt3djIUZT0lhYPd6VzsQ+/Q=", "11977770027" },
                    { 39, null, "aluno28@escola.com", null, null, null, null, 3, null, "Aluno Arthur", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTM5$TWtxhtS9jSNUkmKjp2tRgk29i74w2lVi4Fgv4HuQMEY=", "11977770028" },
                    { 40, null, "aluno29@escola.com", null, null, null, null, 3, null, "Aluno Bianca", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQw$9kyzLd67+M6oGjfbevghZ+fuyXoGhb29YCSCZi6BXyQ=", "11977770029" },
                    { 41, null, "aluno30@escola.com", null, null, null, null, 3, null, "Aluno Caio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQx$f5X/6mMFGRtS2InKkCVFPUYP2wFozwqj0RONe6JzT9E=", "11977770030" },
                    { 42, null, "aluno31@escola.com", null, null, null, null, 3, null, "Aluno Debora", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQy$oeCAYr5m4CRpgyvos/eWRA/uSj9dP6rw0NElasv/xmg=", "11977770031" },
                    { 43, null, "aluno32@escola.com", null, null, null, null, 3, null, "Aluno Enzo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQz$HS3w9dP96z+4u2pZltRMVWPpMoxoFVN7zQ7+1JAl2uU=", "11977770032" },
                    { 44, null, "aluno33@escola.com", null, null, null, null, 3, null, "Aluno Flavia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ0$IcH8MjIVdPL1wLk4/hLU5f8AT46jb0x3essAAlrnYis=", "11977770033" },
                    { 45, null, "aluno34@escola.com", null, null, null, null, 3, null, "Aluno Guilherme", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ1$6BoHSz4mlca42WiwnnBiO8Kx4/Fzi50qWXUgiwWRYYQ=", "11977770034" },
                    { 46, null, "aluno35@escola.com", null, null, null, null, 3, null, "Aluno Helena", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ2$KCkOBFuLcIRIuK7gqE9poyhR9y64Mp9tkeMhdGHSD9s=", "11977770035" },
                    { 47, null, "aluno36@escola.com", null, null, null, null, 3, null, "Aluno Igor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ3$523llIFnguTH/732JxaLkySTHvPrgj0qGknB5+b6aAQ=", "11977770036" },
                    { 48, null, "aluno37@escola.com", null, null, null, null, 3, null, "Aluno Julia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ4$H6x2RcWyBoRkUVvOR6fv45cFxtJyNy6OzhGZQh0H9nw=", "11977770037" },
                    { 49, null, "aluno38@escola.com", null, null, null, null, 3, null, "Aluno Kevin", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTQ5$UZAwt0csoli0DDz99JGO0zcHbjotiyurjaeY5Bx+MhY=", "11977770038" },
                    { 50, null, "aluno39@escola.com", null, null, null, null, 3, null, "Aluno Laura", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTUw$OsU1k3bPx9CXjnoet+lWeWrL5cmfXyXvkKPmSkgbWDQ=", "11977770039" },
                    { 51, null, "aluno40@escola.com", null, null, null, null, 3, null, "Aluno Miguel", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTUx$bLFRqBOpGpNJRBdE8M0dKrnbEJcHo199xzQK8ljibp4=", "11977770040" },
                    { 52, null, "aluno41@escola.com", null, null, null, null, 3, null, "Aluno Natalia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTUy$bhlrrB1RoATCdYSwRREtV56lM1mFl5zGe6YDTACDq+U=", "11977770041" },
                    { 53, null, "aluno42@escola.com", null, null, null, null, 3, null, "Aluno Otavio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTUz$v7QRbY/lWkKDOTy0QHl+LJo5L91EFL22JKh1hKv+/Iw=", "11977770042" },
                    { 54, null, "aluno43@escola.com", null, null, null, null, 3, null, "Aluno Pamela", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU0$R8DZITt7kG5nNoywILy4Po+8tMh9pOEl60McM8FzFpo=", "11977770043" },
                    { 55, null, "aluno44@escola.com", null, null, null, null, 3, null, "Aluno Rafael", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU1$ktiZpAClwksRDnR5dQpmBYW+hqlmwyTr5XI7UkpnFDg=", "11977770044" },
                    { 56, null, "aluno45@escola.com", null, null, null, null, 3, null, "Aluno Sabrina", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU2$yNCqtQ/9oZ3l/XK4PnWZG0grdF2K4I4L8PWma54qcP0=", "11977770045" },
                    { 57, null, "aluno46@escola.com", null, null, null, null, 3, null, "Aluno Tales", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU3$nKVqaSTWwe5iETXB0Mu4mAZGcVVbQIP1ve16FMF7gw0=", "11977770046" },
                    { 58, null, "aluno47@escola.com", null, null, null, null, 3, null, "Aluno Vanessa", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU4$vu48MLao+YpL97uk+qDM5xnGSyVNP2SGqcIadICONE8=", "11977770047" },
                    { 59, null, "aluno48@escola.com", null, null, null, null, 3, null, "Aluno William", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTU5$7yddVxAYXCXv8lhzeSm68lztWTI3KTtSO7Z366apl7E=", "11977770048" },
                    { 60, null, "aluno49@escola.com", null, null, null, null, 3, null, "Aluno Xenia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTYw$bo/lTXoUJILZrrYr36K52anSrEq61mGz+2YyOExN3kc=", "11977770049" },
                    { 61, null, "aluno50@escola.com", null, null, null, null, 3, null, "Aluno Yuri", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTYx$tcj3yNyR2DBeNgwLh5bG39Tqhvto6rHfr9eKgigVMGY=", "11977770050" },
                    { 62, null, "aluno51@escola.com", null, null, null, null, 3, null, "Aluno Alice", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTYy$xK1Vz4xqs3FnNRYO7BVT9ex2wWBsutldX/0j6mac3Lc=", "11977770051" },
                    { 63, null, "aluno52@escola.com", null, null, null, null, 3, null, "Aluno Davi", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTYz$+T1sjjpCHh9oSzWIYiGB2aes4ccgx8T9Am04/7c5+Gk=", "11977770052" },
                    { 64, null, "aluno53@escola.com", null, null, null, null, 3, null, "Aluno Livia", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY0$V+8ywVhDGgHOHl00HJx3iyT5gZplmK36Gyj49XN6Wc4=", "11977770053" },
                    { 65, null, "aluno54@escola.com", null, null, null, null, 3, null, "Aluno Heitor", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY1$0jlkuSrUbulNbVsqs5fJX/1R4PGNSt2HnVE2RzKcGmo=", "11977770054" },
                    { 66, null, "aluno55@escola.com", null, null, null, null, 3, null, "Aluno Luiza", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY2$/C7oZCoIdzKHJTt34+qQjkzETtWOralL5A9kGeYYrg4=", "11977770055" },
                    { 67, null, "aluno56@escola.com", null, null, null, null, 3, null, "Aluno Theo", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY3$uPVkoo19x1Rd6OkpX4yNBKI5ykZjU7aekRhQ/xDkIO8=", "11977770056" },
                    { 68, null, "aluno57@escola.com", null, null, null, null, 3, null, "Aluno Melissa", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY4$afPerjWHZKC7udWfM4YO1I1sXsYRQ3OCt8TkAb+cbuU=", "11977770057" },
                    { 69, null, "aluno58@escola.com", null, null, null, null, 3, null, "Aluno Benicio", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTY5$ii4bHeJa33OdIVjSoNvj/YSmBpCOC2fGVNsIZBdkahU=", "11977770058" },
                    { 70, null, "aluno59@escola.com", null, null, null, null, 3, null, "Aluno Clara", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTcw$mQOg926berZXvmFFSUQiXNiH1zd5qny7985wOp+4Ey0=", "11977770059" },
                    { 71, null, "aluno60@escola.com", null, null, null, null, 3, null, "Aluno Isadora", null, null, null, null, null, null, "PBKDF2-SHA256$100000$dXN1YXJpby1zZWVkLTcx$Be87l5Bhboa2u/LC/jlLevjIcohYWRIqZQfeqc3CH/0=", "11977770060" }
                });

            migrationBuilder.InsertData(
                table: "AlunoTurmaEnsino",
                columns: new[] { "IdAlunoTurmaEnsino", "IdAlunoUsuario", "IdTurmaEnsino", "IdUsuarioResponsavel", "MatriculadoEmUtc" },
                values: new object[,]
                {
                    { 10, 21, 102, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 22, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 23, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 24, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 25, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, 26, 103, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 27, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 28, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 29, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 30, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 31, 104, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 32, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 33, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, 34, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 35, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 36, 105, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 37, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 38, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, 39, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 40, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 41, 106, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 42, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 43, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, 44, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 45, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 46, 107, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 36, 47, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, 48, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, 49, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, 50, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, 51, 108, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 52, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, 53, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, 54, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, 55, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, 56, 109, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 46, 57, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, 58, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, 59, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, 60, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, 61, 201, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, 62, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, 63, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 53, 64, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 54, 65, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 55, 66, 202, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 56, 67, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 57, 68, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 58, 69, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 59, 70, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 60, 71, 203, 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "DataNasc", "IdUsuario", "Nome", "ProfessorId", "Sobrenome" },
                values: new object[,]
                {
                    { 10, "10/10/2014", 21, "Gabriela", 10, "Rodrigues" },
                    { 11, "11/11/2015", 22, "Hugo", 1, "Almeida" },
                    { 12, "12/12/2016", 23, "Isabela", 2, "Nascimento" },
                    { 13, "13/01/2005", 24, "Jonas", 3, "Lima" },
                    { 14, "14/02/2006", 25, "Karina", 4, "Araujo" },
                    { 15, "15/03/2007", 26, "Luis", 5, "Ferreira" },
                    { 16, "16/04/2008", 27, "Manuela", 6, "Carvalho" },
                    { 17, "17/05/2009", 28, "Nicolas", 7, "Ribeiro" },
                    { 18, "18/06/2010", 29, "Olivia", 8, "Martins" },
                    { 19, "19/07/2011", 30, "Pedro", 9, "Rocha" },
                    { 20, "20/08/2012", 31, "Rafaela", 10, "Barbosa" },
                    { 21, "21/09/2013", 32, "Sofia", 1, "Dias" },
                    { 22, "22/10/2014", 33, "Tiago", 2, "Teixeira" },
                    { 23, "23/11/2015", 34, "Ursula", 3, "Correia" },
                    { 24, "24/12/2016", 35, "Victor", 4, "Mendes" },
                    { 25, "25/01/2005", 36, "Wesley", 5, "Cardoso" },
                    { 26, "26/02/2006", 37, "Yasmin", 6, "Ramos" },
                    { 27, "27/03/2007", 38, "Zoe", 7, "Castro" },
                    { 28, "28/04/2008", 39, "Arthur", 8, "Fernandes" },
                    { 29, "01/05/2009", 40, "Bianca", 9, "Moreira" },
                    { 30, "02/06/2010", 41, "Caio", 10, "Moura" },
                    { 31, "03/07/2011", 42, "Debora", 1, "Batista" },
                    { 32, "04/08/2012", 43, "Enzo", 2, "Freitas" },
                    { 33, "05/09/2013", 44, "Flavia", 3, "Monteiro" },
                    { 34, "06/10/2014", 45, "Guilherme", 4, "Campos" },
                    { 35, "07/11/2015", 46, "Helena", 5, "Vieira" },
                    { 36, "08/12/2016", 47, "Igor", 6, "Pinto" },
                    { 37, "09/01/2005", 48, "Julia", 7, "Cavalcanti" },
                    { 38, "10/02/2006", 49, "Kevin", 8, "Farias" },
                    { 39, "11/03/2007", 50, "Laura", 9, "Cunha" },
                    { 40, "12/04/2008", 51, "Miguel", 10, "Duarte" },
                    { 41, "13/05/2009", 52, "Natalia", 1, "Lopes" },
                    { 42, "14/06/2010", 53, "Otavio", 2, "Reis" },
                    { 43, "15/07/2011", 54, "Pamela", 3, "Pires" },
                    { 44, "16/08/2012", 55, "Rafael", 4, "Tavares" },
                    { 45, "17/09/2013", 56, "Sabrina", 5, "Mello" },
                    { 46, "18/10/2014", 57, "Tales", 6, "Assis" },
                    { 47, "19/11/2015", 58, "Vanessa", 7, "Peixoto" },
                    { 48, "20/12/2016", 59, "William", 8, "Nunes" },
                    { 49, "21/01/2005", 60, "Xenia", 9, "Macedo" },
                    { 50, "22/02/2006", 61, "Yuri", 10, "Brito" },
                    { 51, "23/03/2007", 62, "Alice", 1, "Medeiros" },
                    { 52, "24/04/2008", 63, "Davi", 2, "Sales" },
                    { 53, "25/05/2009", 64, "Livia", 3, "Amaral" },
                    { 54, "26/06/2010", 65, "Heitor", 4, "Queiroz" },
                    { 55, "27/07/2011", 66, "Luiza", 5, "Rezende" },
                    { 56, "28/08/2012", 67, "Theo", 6, "Aguiar" },
                    { 57, "01/09/2013", 68, "Melissa", 7, "Moraes" },
                    { 58, "02/10/2014", 69, "Benicio", 8, "Dantas" },
                    { 59, "03/11/2015", 70, "Clara", 9, "Borges" },
                    { 60, "04/12/2016", 71, "Isadora", 10, "Neves" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "AlunoTurmaEnsino",
                keyColumn: "IdAlunoTurmaEnsino",
                keyValue: 60);

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
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Alunos",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 71);

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
