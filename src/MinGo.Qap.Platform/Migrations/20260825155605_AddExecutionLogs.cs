using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinGo.Qap.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchedulerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    JobName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    JobGroup = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AgentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_AgentId",
                table: "ExecutionLogs",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_CreatedAt",
                table: "ExecutionLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_SchedulerName_JobName_JobGroup",
                table: "ExecutionLogs",
                columns: new[] { "SchedulerName", "JobName", "JobGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_StartTime",
                table: "ExecutionLogs",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionLogs");
        }
    }
}
