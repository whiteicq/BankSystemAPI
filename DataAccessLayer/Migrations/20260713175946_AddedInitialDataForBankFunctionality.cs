using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddedInitialDataForBankFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Clients_ClientId",
                table: "BankAccounts");

            migrationBuilder.AlterColumn<long>(
                name: "ClientId",
                table: "BankAccounts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "Address", "BIC", "Title" },
                values: new object[,]
                {
                    { 1L, "г. Минск, ул. Сурганова, 43", "ALFABY2X", "Альфа-Банк" },
                    { 2L, "г. Минск, ул. В.Хоружей 31А", "PJCBBY2X", "Приорбанк" },
                    { 3L, "г. Минск, пр-к Дзержинского, 18", "AKBBBY2X", "Беларусбанк" }
                });

            migrationBuilder.InsertData(
                table: "BankAccounts",
                columns: new[] { "Id", "BankAccountNumber", "BankId", "ClientId", "ClosedAt", "Currency", "MoneyBalance", "OpenedAt", "Status", "Type" },
                values: new object[,]
                {
                    { 1L, "0000000000000000000000000000", 1L, null, null, "BYN", 999999999999m, new DateOnly(2026, 7, 13), "Active", "Current" },
                    { 2L, "1111111111111111111111111111", 2L, null, null, "BYN", 999999999999m, new DateOnly(2026, 7, 13), "Active", "Current" },
                    { 3L, "2222222222222222222222222222", 3L, null, null, "BYN", 999999999999m, new DateOnly(2026, 7, 13), "Active", "Current" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Clients_ClientId",
                table: "BankAccounts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Clients_ClientId",
                table: "BankAccounts");

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.AlterColumn<long>(
                name: "ClientId",
                table: "BankAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Clients_ClientId",
                table: "BankAccounts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
