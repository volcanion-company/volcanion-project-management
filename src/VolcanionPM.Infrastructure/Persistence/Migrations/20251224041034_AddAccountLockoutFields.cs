using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolcanionPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "volcanion_pm",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedLoginAt",
                schema: "volcanion_pm",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndDate",
                schema: "volcanion_pm",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastFailedLoginAt",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEndDate",
                schema: "volcanion_pm",
                table: "Users");
        }
    }
}
