using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinGo.Qap.Platform.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClusterAndDeclarativeJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentInstances");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.RenameColumn(
                name: "ClusterId",
                table: "JobDefinitions",
                newName: "SchedulerName");

            migrationBuilder.RenameIndex(
                name: "IX_JobDefinitions_ClusterId_JobKey",
                table: "JobDefinitions",
                newName: "IX_JobDefinitions_SchedulerName_JobKey");

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "JobDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultJson",
                table: "JobDefinitions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "JobDefinitions");

            migrationBuilder.DropColumn(
                name: "ResultJson",
                table: "JobDefinitions");

            migrationBuilder.RenameColumn(
                name: "SchedulerName",
                table: "JobDefinitions",
                newName: "ClusterId");

            migrationBuilder.RenameIndex(
                name: "IX_JobDefinitions_SchedulerName_JobKey",
                table: "JobDefinitions",
                newName: "IX_JobDefinitions_ClusterId_JobKey");

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AgentUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Env = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastHeartbeat = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", maxLength: 32, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentInstances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClusterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AgentVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    LastHeartbeat = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    QuartzInstanceId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Status = table.Column<int>(type: "integer", maxLength: 32, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentInstances_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_ClusterId",
                table: "AgentInstances",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_LastHeartbeat",
                table: "AgentInstances",
                column: "LastHeartbeat");

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_Status",
                table: "AgentInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_DeletedAt",
                table: "Clusters",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_Env",
                table: "Clusters",
                column: "Env");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_Status",
                table: "Clusters",
                column: "Status");
        }
    }
}
