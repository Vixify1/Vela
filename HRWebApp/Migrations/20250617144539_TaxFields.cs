using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRWebApp.Migrations
{
    /// <inheritdoc />
    public partial class TaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HealthInsuranceDeduction",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IncomeTaxDeduction",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SocialSecurityDeduction",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthInsuranceDeduction",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "IncomeTaxDeduction",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "SocialSecurityDeduction",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "PayrollRecord");
        }
    }
}
