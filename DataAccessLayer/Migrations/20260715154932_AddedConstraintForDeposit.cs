using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddedConstraintForDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Deposit_DepositInterest",
                table: "Deposits",
                sql: "DepositInterest > 5 AND DepositInterest <= 11");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Deposit_DepositInterest",
                table: "Deposits");

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 1L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 13));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 2L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 13));

            migrationBuilder.UpdateData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 3L,
                column: "OpenedAt",
                value: new DateOnly(2026, 7, 13));
        }
    }
}
