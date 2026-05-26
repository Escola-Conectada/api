using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AjustaTabelaArquivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioArquivo_Usuario_IdUsuario",
                table: "UsuarioArquivo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioArquivo",
                table: "UsuarioArquivo");

            migrationBuilder.RenameTable(
                name: "UsuarioArquivo",
                newName: "Arquivo");

            migrationBuilder.RenameColumn(
                name: "IdUsuarioArquivo",
                table: "Arquivo",
                newName: "IdArquivo");

            migrationBuilder.RenameColumn(
                name: "CaminhoRelativo",
                table: "Arquivo",
                newName: "NomeBlob");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioArquivo_IdUsuario",
                table: "Arquivo",
                newName: "IX_Arquivo_IdUsuario");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Arquivo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "TipoArquivo",
                table: "Arquivo",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<long>(
                name: "TamanhoBytes",
                table: "Arquivo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "NomeBlob",
                table: "Arquivo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "NomeOriginal",
                table: "Arquivo",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "Arquivo",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CriadoEmUtc",
                table: "Arquivo",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Arquivo",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Arquivo",
                table: "Arquivo",
                column: "IdArquivo");

            migrationBuilder.AddForeignKey(
                name: "FK_Arquivo_Usuario_IdUsuario",
                table: "Arquivo",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arquivo_Usuario_IdUsuario",
                table: "Arquivo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Arquivo",
                table: "Arquivo");

            migrationBuilder.RenameTable(
                name: "Arquivo",
                newName: "UsuarioArquivo");

            migrationBuilder.RenameColumn(
                name: "IdArquivo",
                table: "UsuarioArquivo",
                newName: "IdUsuarioArquivo");

            migrationBuilder.RenameColumn(
                name: "NomeBlob",
                table: "UsuarioArquivo",
                newName: "CaminhoRelativo");

            migrationBuilder.RenameIndex(
                name: "IX_Arquivo_IdUsuario",
                table: "UsuarioArquivo",
                newName: "IX_UsuarioArquivo_IdUsuario");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "UsuarioArquivo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoArquivo",
                table: "UsuarioArquivo",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TamanhoBytes",
                table: "UsuarioArquivo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NomeOriginal",
                table: "UsuarioArquivo",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "UsuarioArquivo",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CriadoEmUtc",
                table: "UsuarioArquivo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CaminhoRelativo",
                table: "UsuarioArquivo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "UsuarioArquivo",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuarioArquivo",
                table: "UsuarioArquivo",
                column: "IdUsuarioArquivo");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioArquivo_Usuario_IdUsuario",
                table: "UsuarioArquivo",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
