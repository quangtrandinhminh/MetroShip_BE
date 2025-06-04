using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_1502 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ__Shipment__A2A2A54B59D458B2",
                table: "Shipments",
                newName: "IX_Shipments_TrackingCode");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalKm",
                table: "Shipments",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceVnd",
                table: "Parcels",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalKm",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PriceVnd",
                table: "Parcels");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_TrackingCode",
                table: "Shipments",
                newName: "UQ__Shipment__A2A2A54B59D458B2");
        }
    }
}
