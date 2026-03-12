using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HikariNoShisai.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToAgentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Agents",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Agents",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Agents");
        }
    }
}
