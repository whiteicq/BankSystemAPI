using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DropConstraintForDepositTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Deposit_DepositInterest",
                table: "Deposits");

            migrationBuilder.AlterColumn<decimal>(
                name: "DepositInterest",
                table: "Deposits",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "DepositInterest",
                table: "Deposits",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Deposit_DepositInterest",
                table: "Deposits",
                sql: "DepositInterest > 5 AND DepositInterest <= 11");
        }
    }
}
