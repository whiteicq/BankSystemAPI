using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixedAutoDateInEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedAt",
                table: "Transactions",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "Deposits",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "Credits",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "BankAccounts",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 1L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 12));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 2L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 12));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 3L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 12));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedAt",
                table: "Transactions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "Deposits",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "Credits",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "BankAccounts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 1L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 15));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 2L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 15));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 3L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 15));
        }
    }
}
