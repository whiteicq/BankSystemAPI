using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixedLogExceptionWithLengthOfTypeOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypeOperation",
                table: "Logs",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "BankAccounts",
                type: "date",
                nullable: true,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypeOperation",
                table: "Logs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OpenedAt",
                table: "BankAccounts",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)",
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "CAST(GETUTCDATE() AS DATE)");
        }
    }
}
