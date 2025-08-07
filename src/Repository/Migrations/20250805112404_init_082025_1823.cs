using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_1823 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainId",
                table: "ParcelTrackings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "ParcelTrackings");
        }
    }
}
