using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinGo.Qap.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AgentSchedulerRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgentInstances_ClusterId_Url",
                table: "AgentInstances");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AgentVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastHeartbeat = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    LastReportedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchedulerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SchedulerInstanceId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsClustered = table.Column<bool>(type: "boolean", nullable: false),
                    JobStoreType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ThreadPoolType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ThreadPoolSize = table.Column<int>(type: "integer", nullable: false),
                    RunningSince = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NumberOfJobsExecuted = table.Column<int>(type: "integer", nullable: false),
                    PropertiesJson = table.Column<string>(type: "text", nullable: true),
                    FirstReportedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    LastReportedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentSchedulers",
                columns: table => new
                {
                    AgentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchedulerInfoId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSchedulers", x => new { x.AgentId, x.SchedulerInfoId });
                    table.ForeignKey(
                        name: "FK_AgentSchedulers_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentSchedulers_SchedulerInfos_SchedulerInfoId",
                        column: x => x.SchedulerInfoId,
                        principalTable: "SchedulerInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_LastHeartbeat",
                table: "Agents",
                column: "LastHeartbeat");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Status",
                table: "Agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedulers_AgentId",
                table: "AgentSchedulers",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedulers_ReportedAt",
                table: "AgentSchedulers",
                column: "ReportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedulers_SchedulerInfoId",
                table: "AgentSchedulers",
                column: "SchedulerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerInfos_LastReportedAt",
                table: "SchedulerInfos",
                column: "LastReportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerInfos_SchedulerName_SchedulerInstanceId",
                table: "SchedulerInfos",
                columns: new[] { "SchedulerName", "SchedulerInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerInfos_Status",
                table: "SchedulerInfos",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSchedulers");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "SchedulerInfos");

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_ClusterId_Url",
                table: "AgentInstances",
                columns: new[] { "ClusterId", "Url" },
                unique: true);
        }
    }
}
