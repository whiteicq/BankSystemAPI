using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ__passport_number",
                table: "Passports",
                newName: "UQ_passport_number");

            migrationBuilder.RenameIndex(
                name: "UQ__employee_phone_number",
                table: "Employees",
                newName: "UQ_employee_phone_number");

            migrationBuilder.RenameIndex(
                name: "UQ__employee_personell_number",
                table: "Employees",
                newName: "UQ_employee_personell_number");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Deposits",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Credits",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Credit_LoanInterest",
                table: "Credits",
                sql: "LoanInterest >= 14 AND LoanInterest <= 25");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Credit_LoanInterest",
                table: "Credits");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Credits");

            migrationBuilder.RenameIndex(
                name: "UQ_passport_number",
                table: "Passports",
                newName: "UQ__passport_number");

            migrationBuilder.RenameIndex(
                name: "UQ_employee_phone_number",
                table: "Employees",
                newName: "UQ__employee_phone_number");

            migrationBuilder.RenameIndex(
                name: "UQ_employee_personell_number",
                table: "Employees",
                newName: "UQ__employee_personell_number");
        }
    }
}
