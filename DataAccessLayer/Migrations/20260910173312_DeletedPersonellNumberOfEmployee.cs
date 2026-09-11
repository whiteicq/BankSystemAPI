using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DeletedPersonellNumberOfEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_employee_personell_number",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PersonellNumber",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonellNumber",
                table: "Employees",
                type: "nchar(10)",
                fixedLength: true,
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UQ_employee_personell_number",
                table: "Employees",
                column: "PersonellNumber",
                unique: true);
        }
    }
}
