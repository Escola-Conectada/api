using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstruturaEnsinoDisciplinas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Disciplina_IdProfessorUsuario_Nome",
                table: "Disciplina");

            migrationBuilder.AlterColumn<int>(
                name: "IdProfessorUsuario",
                table: "Disciplina",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdAreaConhecimento",
                table: "Disciplina",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTurmaEnsino",
                table: "Disciplina",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MatriculaFacultativa",
                table: "Disciplina",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Observacao",
                table: "Disciplina",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OfertaObrigatoria",
                table: "Disciplina",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Disciplina",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TipoEnsino",
                columns: table => new
                {
                    IdTipoEnsino = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoEnsino", x => x.IdTipoEnsino);
                });

            migrationBuilder.CreateTable(
                name: "AreaConhecimento",
                columns: table => new
                {
                    IdAreaConhecimento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoEnsino = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaConhecimento", x => x.IdAreaConhecimento);
                    table.ForeignKey(
                        name: "FK_AreaConhecimento_TipoEnsino_IdTipoEnsino",
                        column: x => x.IdTipoEnsino,
                        principalTable: "TipoEnsino",
                        principalColumn: "IdTipoEnsino",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurmaEnsino",
                columns: table => new
                {
                    IdTurmaEnsino = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoEnsino = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaEnsino", x => x.IdTurmaEnsino);
                    table.ForeignKey(
                        name: "FK_TurmaEnsino_TipoEnsino_IdTipoEnsino",
                        column: x => x.IdTipoEnsino,
                        principalTable: "TipoEnsino",
                        principalColumn: "IdTipoEnsino",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TipoEnsino",
                columns: new[] { "IdTipoEnsino", "Nome", "Ordem" },
                values: new object[,]
                {
                    { 1, "Ensino Fundamental", 1 },
                    { 2, "Ensino Médio", 2 }
                });

            migrationBuilder.InsertData(
                table: "AreaConhecimento",
                columns: new[] { "IdAreaConhecimento", "IdTipoEnsino", "Nome", "Ordem" },
                values: new object[,]
                {
                    { 101, 1, "Linguagens", 1 },
                    { 102, 1, "Matemática", 2 },
                    { 103, 1, "Ciências da Natureza", 3 },
                    { 104, 1, "Ciências Humanas", 4 },
                    { 105, 1, "Ensino Religioso", 5 },
                    { 201, 2, "Linguagens e suas Tecnologias", 1 },
                    { 202, 2, "Matemática e suas Tecnologias", 2 },
                    { 203, 2, "Ciências da Natureza e suas Tecnologias", 3 },
                    { 204, 2, "Ciências Humanas e Sociais Aplicadas", 4 }
                });

            migrationBuilder.InsertData(
                table: "TurmaEnsino",
                columns: new[] { "IdTurmaEnsino", "Codigo", "IdTipoEnsino", "Nome", "Ordem" },
                values: new object[,]
                {
                    { 101, "EF1", 1, "1º ano", 1 },
                    { 102, "EF2", 1, "2º ano", 2 },
                    { 103, "EF3", 1, "3º ano", 3 },
                    { 104, "EF4", 1, "4º ano", 4 },
                    { 105, "EF5", 1, "5º ano", 5 },
                    { 106, "EF6", 1, "6º ano", 6 },
                    { 107, "EF7", 1, "7º ano", 7 },
                    { 108, "EF8", 1, "8º ano", 8 },
                    { 109, "EF9", 1, "9º ano", 9 },
                    { 201, "EM1", 2, "1ª série", 1 },
                    { 202, "EM2", 2, "2ª série", 2 },
                    { 203, "EM3", 2, "3ª série", 3 }
                });

            migrationBuilder.InsertData(
                table: "Disciplina",
                columns: new[] { "IdDisciplina", "IdAreaConhecimento", "IdProfessorUsuario", "IdTurmaEnsino", "MatriculaFacultativa", "Nome", "Observacao", "OfertaObrigatoria", "Ordem" },
                values: new object[,]
                {
                    { 1001, 101, null, 101, false, "Língua Portuguesa", null, true, 1 },
                    { 1002, 101, null, 101, false, "Arte", null, true, 2 },
                    { 1003, 101, null, 101, false, "Educação Física", null, true, 3 },
                    { 1004, 102, null, 101, false, "Matemática", null, true, 1 },
                    { 1005, 103, null, 101, false, "Ciências", null, true, 1 },
                    { 1006, 104, null, 101, false, "História", null, true, 1 },
                    { 1007, 104, null, 101, false, "Geografia", null, true, 2 },
                    { 1008, 105, null, 101, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1009, 101, null, 102, false, "Língua Portuguesa", null, true, 1 },
                    { 1010, 101, null, 102, false, "Arte", null, true, 2 },
                    { 1011, 101, null, 102, false, "Educação Física", null, true, 3 },
                    { 1012, 102, null, 102, false, "Matemática", null, true, 1 },
                    { 1013, 103, null, 102, false, "Ciências", null, true, 1 },
                    { 1014, 104, null, 102, false, "História", null, true, 1 },
                    { 1015, 104, null, 102, false, "Geografia", null, true, 2 },
                    { 1016, 105, null, 102, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1017, 101, null, 103, false, "Língua Portuguesa", null, true, 1 },
                    { 1018, 101, null, 103, false, "Arte", null, true, 2 },
                    { 1019, 101, null, 103, false, "Educação Física", null, true, 3 },
                    { 1020, 102, null, 103, false, "Matemática", null, true, 1 },
                    { 1021, 103, null, 103, false, "Ciências", null, true, 1 },
                    { 1022, 104, null, 103, false, "História", null, true, 1 },
                    { 1023, 104, null, 103, false, "Geografia", null, true, 2 },
                    { 1024, 105, null, 103, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1025, 101, null, 104, false, "Língua Portuguesa", null, true, 1 },
                    { 1026, 101, null, 104, false, "Arte", null, true, 2 },
                    { 1027, 101, null, 104, false, "Educação Física", null, true, 3 },
                    { 1028, 102, null, 104, false, "Matemática", null, true, 1 },
                    { 1029, 103, null, 104, false, "Ciências", null, true, 1 },
                    { 1030, 104, null, 104, false, "História", null, true, 1 },
                    { 1031, 104, null, 104, false, "Geografia", null, true, 2 },
                    { 1032, 105, null, 104, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1033, 101, null, 105, false, "Língua Portuguesa", null, true, 1 },
                    { 1034, 101, null, 105, false, "Arte", null, true, 2 },
                    { 1035, 101, null, 105, false, "Educação Física", null, true, 3 },
                    { 1036, 102, null, 105, false, "Matemática", null, true, 1 },
                    { 1037, 103, null, 105, false, "Ciências", null, true, 1 },
                    { 1038, 104, null, 105, false, "História", null, true, 1 },
                    { 1039, 104, null, 105, false, "Geografia", null, true, 2 },
                    { 1040, 105, null, 105, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1041, 101, null, 106, false, "Língua Portuguesa", null, true, 1 },
                    { 1042, 101, null, 106, false, "Arte", null, true, 2 },
                    { 1043, 101, null, 106, false, "Educação Física", null, true, 3 },
                    { 1044, 101, null, 106, false, "Língua Inglesa", "Obrigatória a partir do 6º ano.", true, 4 },
                    { 1045, 102, null, 106, false, "Matemática", null, true, 1 },
                    { 1046, 103, null, 106, false, "Ciências", null, true, 1 },
                    { 1047, 104, null, 106, false, "História", null, true, 1 },
                    { 1048, 104, null, 106, false, "Geografia", null, true, 2 },
                    { 1049, 105, null, 106, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1050, 101, null, 107, false, "Língua Portuguesa", null, true, 1 },
                    { 1051, 101, null, 107, false, "Arte", null, true, 2 },
                    { 1052, 101, null, 107, false, "Educação Física", null, true, 3 },
                    { 1053, 101, null, 107, false, "Língua Inglesa", "Obrigatória a partir do 6º ano.", true, 4 },
                    { 1054, 102, null, 107, false, "Matemática", null, true, 1 },
                    { 1055, 103, null, 107, false, "Ciências", null, true, 1 },
                    { 1056, 104, null, 107, false, "História", null, true, 1 },
                    { 1057, 104, null, 107, false, "Geografia", null, true, 2 },
                    { 1058, 105, null, 107, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1059, 101, null, 108, false, "Língua Portuguesa", null, true, 1 },
                    { 1060, 101, null, 108, false, "Arte", null, true, 2 },
                    { 1061, 101, null, 108, false, "Educação Física", null, true, 3 },
                    { 1062, 101, null, 108, false, "Língua Inglesa", "Obrigatória a partir do 6º ano.", true, 4 },
                    { 1063, 102, null, 108, false, "Matemática", null, true, 1 },
                    { 1064, 103, null, 108, false, "Ciências", null, true, 1 },
                    { 1065, 104, null, 108, false, "História", null, true, 1 },
                    { 1066, 104, null, 108, false, "Geografia", null, true, 2 },
                    { 1067, 105, null, 108, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1068, 101, null, 109, false, "Língua Portuguesa", null, true, 1 },
                    { 1069, 101, null, 109, false, "Arte", null, true, 2 },
                    { 1070, 101, null, 109, false, "Educação Física", null, true, 3 },
                    { 1071, 101, null, 109, false, "Língua Inglesa", "Obrigatória a partir do 6º ano.", true, 4 },
                    { 1072, 102, null, 109, false, "Matemática", null, true, 1 },
                    { 1073, 103, null, 109, false, "Ciências", null, true, 1 },
                    { 1074, 104, null, 109, false, "História", null, true, 1 },
                    { 1075, 104, null, 109, false, "Geografia", null, true, 2 },
                    { 1076, 105, null, 109, true, "Ensino Religioso", "Oferta obrigatória pela escola, matrícula facultativa para o aluno.", true, 1 },
                    { 1077, 201, null, 201, false, "Língua Portuguesa", null, true, 1 },
                    { 1078, 201, null, 201, false, "Literatura", null, true, 2 },
                    { 1079, 201, null, 201, false, "Arte", null, true, 3 },
                    { 1080, 201, null, 201, false, "Educação Física", null, true, 4 },
                    { 1081, 201, null, 201, false, "Língua Inglesa", null, true, 5 },
                    { 1082, 202, null, 201, false, "Matemática", null, true, 1 },
                    { 1083, 203, null, 201, false, "Física", null, true, 1 },
                    { 1084, 203, null, 201, false, "Química", null, true, 2 },
                    { 1085, 203, null, 201, false, "Biologia", null, true, 3 },
                    { 1086, 204, null, 201, false, "História", null, true, 1 },
                    { 1087, 204, null, 201, false, "Geografia", null, true, 2 },
                    { 1088, 204, null, 201, false, "Filosofia", null, true, 3 },
                    { 1089, 204, null, 201, false, "Sociologia", null, true, 4 },
                    { 1090, 201, null, 202, false, "Língua Portuguesa", null, true, 1 },
                    { 1091, 201, null, 202, false, "Literatura", null, true, 2 },
                    { 1092, 201, null, 202, false, "Arte", null, true, 3 },
                    { 1093, 201, null, 202, false, "Educação Física", null, true, 4 },
                    { 1094, 201, null, 202, false, "Língua Inglesa", null, true, 5 },
                    { 1095, 202, null, 202, false, "Matemática", null, true, 1 },
                    { 1096, 203, null, 202, false, "Física", null, true, 1 },
                    { 1097, 203, null, 202, false, "Química", null, true, 2 },
                    { 1098, 203, null, 202, false, "Biologia", null, true, 3 },
                    { 1099, 204, null, 202, false, "História", null, true, 1 },
                    { 1100, 204, null, 202, false, "Geografia", null, true, 2 },
                    { 1101, 204, null, 202, false, "Filosofia", null, true, 3 },
                    { 1102, 204, null, 202, false, "Sociologia", null, true, 4 },
                    { 1103, 201, null, 203, false, "Língua Portuguesa", null, true, 1 },
                    { 1104, 201, null, 203, false, "Literatura", null, true, 2 },
                    { 1105, 201, null, 203, false, "Arte", null, true, 3 },
                    { 1106, 201, null, 203, false, "Educação Física", null, true, 4 },
                    { 1107, 201, null, 203, false, "Língua Inglesa", null, true, 5 },
                    { 1108, 202, null, 203, false, "Matemática", null, true, 1 },
                    { 1109, 203, null, 203, false, "Física", null, true, 1 },
                    { 1110, 203, null, 203, false, "Química", null, true, 2 },
                    { 1111, 203, null, 203, false, "Biologia", null, true, 3 },
                    { 1112, 204, null, 203, false, "História", null, true, 1 },
                    { 1113, 204, null, 203, false, "Geografia", null, true, 2 },
                    { 1114, 204, null, 203, false, "Filosofia", null, true, 3 },
                    { 1115, 204, null, 203, false, "Sociologia", null, true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Disciplina_IdAreaConhecimento",
                table: "Disciplina",
                column: "IdAreaConhecimento");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplina_IdProfessorUsuario_IdTurmaEnsino_Nome",
                table: "Disciplina",
                columns: new[] { "IdProfessorUsuario", "IdTurmaEnsino", "Nome" },
                unique: true,
                filter: "[IdProfessorUsuario] IS NOT NULL AND [IdTurmaEnsino] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplina_IdTurmaEnsino",
                table: "Disciplina",
                column: "IdTurmaEnsino");

            migrationBuilder.CreateIndex(
                name: "IX_AreaConhecimento_IdTipoEnsino_Nome",
                table: "AreaConhecimento",
                columns: new[] { "IdTipoEnsino", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoEnsino_Nome",
                table: "TipoEnsino",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurmaEnsino_IdTipoEnsino_Codigo",
                table: "TurmaEnsino",
                columns: new[] { "IdTipoEnsino", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplina_AreaConhecimento_IdAreaConhecimento",
                table: "Disciplina",
                column: "IdAreaConhecimento",
                principalTable: "AreaConhecimento",
                principalColumn: "IdAreaConhecimento",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplina_TurmaEnsino_IdTurmaEnsino",
                table: "Disciplina",
                column: "IdTurmaEnsino",
                principalTable: "TurmaEnsino",
                principalColumn: "IdTurmaEnsino",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplina_AreaConhecimento_IdAreaConhecimento",
                table: "Disciplina");

            migrationBuilder.DropForeignKey(
                name: "FK_Disciplina_TurmaEnsino_IdTurmaEnsino",
                table: "Disciplina");

            migrationBuilder.DropTable(
                name: "AreaConhecimento");

            migrationBuilder.DropTable(
                name: "TurmaEnsino");

            migrationBuilder.DropTable(
                name: "TipoEnsino");

            migrationBuilder.DropIndex(
                name: "IX_Disciplina_IdAreaConhecimento",
                table: "Disciplina");

            migrationBuilder.DropIndex(
                name: "IX_Disciplina_IdProfessorUsuario_IdTurmaEnsino_Nome",
                table: "Disciplina");

            migrationBuilder.DropIndex(
                name: "IX_Disciplina_IdTurmaEnsino",
                table: "Disciplina");

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1005);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1006);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1007);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1008);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1009);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1010);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1011);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1012);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1013);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1014);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1015);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1016);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1017);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1018);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1019);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1020);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1021);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1022);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1023);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1024);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1025);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1026);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1027);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1028);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1029);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1030);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1031);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1032);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1033);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1034);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1035);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1036);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1037);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1038);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1039);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1040);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1041);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1042);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1043);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1044);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1045);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1046);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1047);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1048);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1049);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1050);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1051);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1052);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1053);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1054);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1055);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1056);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1057);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1058);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1059);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1060);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1061);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1062);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1063);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1064);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1065);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1066);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1067);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1068);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1069);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1070);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1071);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1072);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1073);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1074);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1075);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1076);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1077);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1078);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1079);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1080);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1081);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1082);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1083);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1084);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1085);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1086);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1087);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1088);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1089);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1090);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1091);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1092);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1093);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1094);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1095);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1096);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1097);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1098);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1099);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1100);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1101);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1102);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1103);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1104);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1105);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1106);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1107);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1108);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1109);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1110);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1111);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1112);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1113);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1114);

            migrationBuilder.DeleteData(
                table: "Disciplina",
                keyColumn: "IdDisciplina",
                keyValue: 1115);

            migrationBuilder.DropColumn(
                name: "IdAreaConhecimento",
                table: "Disciplina");

            migrationBuilder.DropColumn(
                name: "IdTurmaEnsino",
                table: "Disciplina");

            migrationBuilder.DropColumn(
                name: "MatriculaFacultativa",
                table: "Disciplina");

            migrationBuilder.DropColumn(
                name: "Observacao",
                table: "Disciplina");

            migrationBuilder.DropColumn(
                name: "OfertaObrigatoria",
                table: "Disciplina");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Disciplina");

            migrationBuilder.AlterColumn<int>(
                name: "IdProfessorUsuario",
                table: "Disciplina",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disciplina_IdProfessorUsuario_Nome",
                table: "Disciplina",
                columns: new[] { "IdProfessorUsuario", "Nome" },
                unique: true);
        }
    }
}
