using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class OnetoOneForBankAccountAndCreditWithDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BankAccountId",
                table: "Deposits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BankAccountId",
                table: "Credits",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_BankAccountId",
                table: "Deposits",
                column: "BankAccountId",
                unique: true,
                filter: "[BankAccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Credits_BankAccountId",
                table: "Credits",
                column: "BankAccountId",
                unique: true,
                filter: "[BankAccountId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Credits_BankAccounts_BankAccountId",
                table: "Credits",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_BankAccounts_BankAccountId",
                table: "Deposits",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credits_BankAccounts_BankAccountId",
                table: "Credits");

            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_BankAccounts_BankAccountId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_BankAccountId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Credits_BankAccountId",
                table: "Credits");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "Credits");
        }
    }
}
