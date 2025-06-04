using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_062025_2140 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Parcels_ParcelId",
                table: "ShipmentTrackings");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Stations_StationId",
                table: "ShipmentTrackings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShipmentTrackings",
                table: "ShipmentTrackings");

            migrationBuilder.RenameTable(
                name: "ShipmentTrackings",
                newName: "ParcelTrackings");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ParcelTrackings",
                newName: "IX_ParcelTrackings_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_ParcelId",
                table: "ParcelTrackings",
                newName: "IX_ParcelTrackings_ParcelId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_Id",
                table: "ParcelTrackings",
                newName: "IX_ParcelTrackings_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_CreatedAt",
                table: "ParcelTrackings",
                newName: "IX_ParcelTrackings_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelTrackings",
                table: "ParcelTrackings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParcelTrackings_Parcels_ParcelId",
                table: "ParcelTrackings",
                column: "ParcelId",
                principalTable: "Parcels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParcelTrackings_Stations_StationId",
                table: "ParcelTrackings",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParcelTrackings_Parcels_ParcelId",
                table: "ParcelTrackings");

            migrationBuilder.DropForeignKey(
                name: "FK_ParcelTrackings_Stations_StationId",
                table: "ParcelTrackings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelTrackings",
                table: "ParcelTrackings");

            migrationBuilder.RenameTable(
                name: "ParcelTrackings",
                newName: "ShipmentTrackings");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelTrackings_StationId",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_StationId");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelTrackings_ParcelId",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_ParcelId");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelTrackings_Id",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelTrackings_CreatedAt",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShipmentTrackings",
                table: "ShipmentTrackings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentTrackings_Parcels_ParcelId",
                table: "ShipmentTrackings",
                column: "ParcelId",
                principalTable: "Parcels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentTrackings_Stations_StationId",
                table: "ShipmentTrackings",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }
    }
}
