using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRWebApp.Migrations
{
    /// <inheritdoc />
    public partial class Payroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Employee");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PayrollRecord",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayHours",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayPay",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCalculated",
                table: "PayrollRecord",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardHours",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardPay",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SundayHours",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SundayPay",
                table: "PayrollRecord",
                type: "decimal(22,6)",
                precision: 22,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PayrollRecord",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "HolidayHours",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "HolidayPay",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "IsCalculated",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "StandardHours",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "StandardPay",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "SundayHours",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "SundayPay",
                table: "PayrollRecord");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayrollRecord");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Employee",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
