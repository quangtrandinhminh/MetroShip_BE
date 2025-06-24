using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_062025_2043 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxHeadwayMin",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "MinHeadwayMin",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "TopSpeedKmH",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "TopSpeedUdgKmH",
                table: "MetroLines");

            migrationBuilder.RenameColumn(
                name: "CapacityPerCarriage",
                table: "MetroTrains",
                newName: "NumberOfCarriages");

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageHeightMeter",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageLengthMeter",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageWidthMeter",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxVolumePerCarriageM3",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeightPerCarriageKg",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TopSpeedKmH",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TopSpeedUdgKmH",
                table: "MetroTrains",
                type: "numeric(8,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarriageHeightMeter",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "CarriageLengthMeter",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "CarriageWidthMeter",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "MaxVolumePerCarriageM3",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "MaxWeightPerCarriageKg",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "TopSpeedKmH",
                table: "MetroTrains");

            migrationBuilder.DropColumn(
                name: "TopSpeedUdgKmH",
                table: "MetroTrains");

            migrationBuilder.RenameColumn(
                name: "NumberOfCarriages",
                table: "MetroTrains",
                newName: "CapacityPerCarriage");

            migrationBuilder.AddColumn<int>(
                name: "MaxHeadwayMin",
                table: "MetroLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinHeadwayMin",
                table: "MetroLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopSpeedKmH",
                table: "MetroLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopSpeedUdgKmH",
                table: "MetroLines",
                type: "integer",
                nullable: true);
        }
    }
}
