using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HikariNoShisai.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFormattedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedAtFormatted",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAtFormatted",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedAtFormatted",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAtFormatted",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedAtFormatted",
                table: "AgentTerminals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAtFormatted",
                table: "AgentTerminals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedAtFormatted",
                table: "AgentStatusLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAtFormatted",
                table: "AgentStatusLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedAtFormatted",
                table: "Agents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAtFormatted",
                table: "Agents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtFormatted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAtFormatted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAtFormatted",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtFormatted",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CreatedAtFormatted",
                table: "AgentTerminals");

            migrationBuilder.DropColumn(
                name: "UpdatedAtFormatted",
                table: "AgentTerminals");

            migrationBuilder.DropColumn(
                name: "CreatedAtFormatted",
                table: "AgentStatusLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtFormatted",
                table: "AgentStatusLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAtFormatted",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "UpdatedAtFormatted",
                table: "Agents");
        }
    }
}
