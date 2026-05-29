using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCOLA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountDeletionAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExclusaoContaMotivo",
                table: "Usuario",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExclusaoContaSolicitadaEmUtc",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetSenhaTokenCriadoEmUtc",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetSenhaTokenExpiraEmUtc",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetSenhaTokenHash",
                table: "Usuario",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 1,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 2,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 3,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 4,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 5,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 6,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 7,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 8,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 9,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 10,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 11,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 12,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 13,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 14,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 15,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 16,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 17,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 18,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 19,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 20,
                columns: new[] { "ExclusaoContaMotivo", "ExclusaoContaSolicitadaEmUtc", "ResetSenhaTokenCriadoEmUtc", "ResetSenhaTokenExpiraEmUtc", "ResetSenhaTokenHash" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExclusaoContaMotivo",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ExclusaoContaSolicitadaEmUtc",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ResetSenhaTokenCriadoEmUtc",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ResetSenhaTokenExpiraEmUtc",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ResetSenhaTokenHash",
                table: "Usuario");
        }
    }
}
