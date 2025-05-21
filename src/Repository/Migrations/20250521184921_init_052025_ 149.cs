using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_149 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Stations__A25C5AA7E98BD6EC",
                table: "Stations");

            migrationBuilder.RenameIndex(
                name: "UQ__Routes__A25C5AA7CC95834E",
                table: "Routes",
                newName: "IX_Routes_RouteCode");

            migrationBuilder.AddColumn<bool>(
                name: "IsMultiLine",
                table: "Stations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EstimatedDeparture",
                table: "ShipmentItineraries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EstimatedArrival",
                table: "ShipmentItineraries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "EstMinute",
                table: "ShipmentItineraries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationCode_RegionId",
                table: "Stations",
                columns: new[] { "StationCode", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes",
                columns: new[] { "LineId", "FromStationId", "ToStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetroLines_LineCode_RegionId",
                table: "MetroLines",
                columns: new[] { "LineCode", "RegionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stations_StationCode_RegionId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_MetroLines_LineCode_RegionId",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "IsMultiLine",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "EstMinute",
                table: "ShipmentItineraries");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_RouteCode",
                table: "Routes",
                newName: "UQ__Routes__A25C5AA7CC95834E");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EstimatedDeparture",
                table: "ShipmentItineraries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EstimatedArrival",
                table: "ShipmentItineraries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Stations__A25C5AA7E98BD6EC",
                table: "Stations",
                column: "StationCode",
                unique: true);
        }
    }
}
