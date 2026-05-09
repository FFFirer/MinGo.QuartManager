using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinGo.Qap.Platform.Migrations
{
    /// <inheritdoc />
    public partial class ChangeJobKeyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Group",
                table: "JobDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "JobDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JobDefinitions_SchedulerName_Group_Name",
                table: "JobDefinitions",
                columns: new[] { "SchedulerName", "Group", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobDefinitions_SchedulerName_Group_Name",
                table: "JobDefinitions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "JobDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Group",
                table: "JobDefinitions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
