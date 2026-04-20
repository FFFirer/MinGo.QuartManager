using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinGo.Qap.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentInstancesAndModifyClusters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentInstances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClusterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuartzInstanceId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AgentVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_AgentInstances_ClusterId_Url",
                table: "AgentInstances",
                columns: new[] { "ClusterId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_LastHeartbeat",
                table: "AgentInstances",
                column: "LastHeartbeat");

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstances_Status",
                table: "AgentInstances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentInstances");
        }
    }
}
