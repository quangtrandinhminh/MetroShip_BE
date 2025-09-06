using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_092025_028 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetroLines_Regions_RegionId",
                table: "MetroLines");

            migrationBuilder.DropForeignKey(
                name: "FK_MetroTrains_MetroLines_LineId",
                table: "MetroTrains");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_MetroLines_LineId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Stations_FromStationId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Stations_ToStationId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItineraries_Routes_RouteId",
                table: "ShipmentItineraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Routes",
                table: "Routes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetroLines",
                table: "MetroLines");

            migrationBuilder.RenameTable(
                name: "Routes",
                newName: "RouteStation");

            migrationBuilder.RenameTable(
                name: "MetroLines",
                newName: "MetroRoute");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_ToStationId",
                table: "RouteStation",
                newName: "IX_RouteStation_ToStationId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "RouteStation",
                newName: "IX_RouteStation_LineId_FromStationId_ToStationId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_LineId",
                table: "RouteStation",
                newName: "IX_RouteStation_LineId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_Id",
                table: "RouteStation",
                newName: "IX_RouteStation_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_FromStationId",
                table: "RouteStation",
                newName: "IX_RouteStation_FromStationId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_CreatedAt",
                table: "RouteStation",
                newName: "IX_RouteStation_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_RegionId",
                table: "MetroRoute",
                newName: "IX_MetroRoute_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_LineCode_RegionId",
                table: "MetroRoute",
                newName: "IX_MetroRoute_LineCode_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_Id",
                table: "MetroRoute",
                newName: "IX_MetroRoute_Id");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_CreatedAt",
                table: "MetroRoute",
                newName: "IX_MetroRoute_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RouteStation",
                table: "RouteStation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetroRoute",
                table: "MetroRoute",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetroRoute_Regions_RegionId",
                table: "MetroRoute",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetroTrains_MetroRoute_LineId",
                table: "MetroTrains",
                column: "LineId",
                principalTable: "MetroRoute",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStation_MetroRoute_LineId",
                table: "RouteStation",
                column: "LineId",
                principalTable: "MetroRoute",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStation_Stations_FromStationId",
                table: "RouteStation",
                column: "FromStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStation_Stations_ToStationId",
                table: "RouteStation",
                column: "ToStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItineraries_RouteStation_RouteId",
                table: "ShipmentItineraries",
                column: "RouteId",
                principalTable: "RouteStation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetroRoute_Regions_RegionId",
                table: "MetroRoute");

            migrationBuilder.DropForeignKey(
                name: "FK_MetroTrains_MetroRoute_LineId",
                table: "MetroTrains");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteStation_MetroRoute_LineId",
                table: "RouteStation");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteStation_Stations_FromStationId",
                table: "RouteStation");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteStation_Stations_ToStationId",
                table: "RouteStation");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItineraries_RouteStation_RouteId",
                table: "ShipmentItineraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RouteStation",
                table: "RouteStation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetroRoute",
                table: "MetroRoute");

            migrationBuilder.RenameTable(
                name: "RouteStation",
                newName: "Routes");

            migrationBuilder.RenameTable(
                name: "MetroRoute",
                newName: "MetroLines");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_ToStationId",
                table: "Routes",
                newName: "IX_Routes_ToStationId");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_LineId_FromStationId_ToStationId",
                table: "Routes",
                newName: "IX_Routes_LineId_FromStationId_ToStationId");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_LineId",
                table: "Routes",
                newName: "IX_Routes_LineId");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_Id",
                table: "Routes",
                newName: "IX_Routes_Id");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_FromStationId",
                table: "Routes",
                newName: "IX_Routes_FromStationId");

            migrationBuilder.RenameIndex(
                name: "IX_RouteStation_CreatedAt",
                table: "Routes",
                newName: "IX_Routes_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_MetroRoute_RegionId",
                table: "MetroLines",
                newName: "IX_MetroLines_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_MetroRoute_LineCode_RegionId",
                table: "MetroLines",
                newName: "IX_MetroLines_LineCode_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_MetroRoute_Id",
                table: "MetroLines",
                newName: "IX_MetroLines_Id");

            migrationBuilder.RenameIndex(
                name: "IX_MetroRoute_CreatedAt",
                table: "MetroLines",
                newName: "IX_MetroLines_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Routes",
                table: "Routes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetroLines",
                table: "MetroLines",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetroLines_Regions_RegionId",
                table: "MetroLines",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetroTrains_MetroLines_LineId",
                table: "MetroTrains",
                column: "LineId",
                principalTable: "MetroLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_MetroLines_LineId",
                table: "Routes",
                column: "LineId",
                principalTable: "MetroLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Stations_FromStationId",
                table: "Routes",
                column: "FromStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Stations_ToStationId",
                table: "Routes",
                column: "ToStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItineraries_Routes_RouteId",
                table: "ShipmentItineraries",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
