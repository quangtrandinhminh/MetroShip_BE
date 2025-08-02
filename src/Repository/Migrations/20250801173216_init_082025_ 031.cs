using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_031 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackResponse",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackResponseBy",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartReceiveAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentShipmentStatus",
                table: "ParcelTrackings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStationId",
                table: "MetroTrains",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "FeedbackResponse",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "FeedbackResponseBy",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "StartReceiveAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CurrentShipmentStatus",
                table: "ParcelTrackings");

            migrationBuilder.DropColumn(
                name: "CurrentStationId",
                table: "MetroTrains");
        }
    }
}
