using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolcanionPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "volcanion_pm",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiryDate",
                schema: "volcanion_pm",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive_Role",
                schema: "volcanion_pm",
                table: "Users",
                columns: new[] { "IsActive", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId_IsActive",
                schema: "volcanion_pm",
                table: "Users",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId_Role",
                schema: "volcanion_pm",
                table: "Users",
                columns: new[] { "OrganizationId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                schema: "volcanion_pm",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_IsBillable",
                schema: "volcanion_pm",
                table: "TimeEntries",
                column: "IsBillable");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_TaskId_Date",
                schema: "volcanion_pm",
                table: "TimeEntries",
                columns: new[] { "TaskId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_Date",
                schema: "volcanion_pm",
                table: "TimeEntries",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_IsBillable",
                schema: "volcanion_pm",
                table: "TimeEntries",
                columns: new[] { "UserId", "IsBillable" });

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId_Status",
                schema: "volcanion_pm",
                table: "Sprints",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_AssignedToId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "AssignedToId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId_AssignedToId",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "ProjectId", "AssignedToId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId_Priority",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "ProjectId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId_Status_DueDate",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "ProjectId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_SprintId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "SprintId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_Status_Priority",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_Type",
                schema: "volcanion_pm",
                table: "ProjectTasks",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Priority",
                schema: "volcanion_pm",
                table: "Projects",
                columns: new[] { "OrganizationId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Status",
                schema: "volcanion_pm",
                table: "Projects",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Status_CreatedAt",
                schema: "volcanion_pm",
                table: "Projects",
                columns: new[] { "OrganizationId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerId_Status",
                schema: "volcanion_pm",
                table: "Projects",
                columns: new[] { "ProjectManagerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status_Priority",
                schema: "volcanion_pm",
                table: "Projects",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AssignedToId_Status",
                schema: "volcanion_pm",
                table: "Issues",
                columns: new[] { "AssignedToId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId_Severity",
                schema: "volcanion_pm",
                table: "Issues",
                columns: new[] { "ProjectId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId_Status",
                schema: "volcanion_pm",
                table: "Issues",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Status_Severity",
                schema: "volcanion_pm",
                table: "Issues",
                columns: new[] { "Status", "Severity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive_Role",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId_IsActive",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId_Role",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_IsBillable",
                schema: "volcanion_pm",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_TaskId_Date",
                schema: "volcanion_pm",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId_Date",
                schema: "volcanion_pm",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId_IsBillable",
                schema: "volcanion_pm",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_ProjectId_Status",
                schema: "volcanion_pm",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_AssignedToId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ProjectId_AssignedToId",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ProjectId_Priority",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ProjectId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ProjectId_Status_DueDate",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_SprintId_Status",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_Status_Priority",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_Type",
                schema: "volcanion_pm",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Priority",
                schema: "volcanion_pm",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Status",
                schema: "volcanion_pm",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Status_CreatedAt",
                schema: "volcanion_pm",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectManagerId_Status",
                schema: "volcanion_pm",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status_Priority",
                schema: "volcanion_pm",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Issues_AssignedToId_Status",
                schema: "volcanion_pm",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ProjectId_Severity",
                schema: "volcanion_pm",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ProjectId_Status",
                schema: "volcanion_pm",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Status_Severity",
                schema: "volcanion_pm",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "volcanion_pm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiryDate",
                schema: "volcanion_pm",
                table: "Users");
        }
    }
}
