using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_2210 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ShipmentItineraries",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShipmentTracking",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<string>(type: "text", nullable: false),
                    CurrentShipmentStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentTracking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentTracking_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTracking_CreatedAt",
                table: "ShipmentTracking",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTracking_Id",
                table: "ShipmentTracking",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTracking_ShipmentId",
                table: "ShipmentTracking",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentTracking");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ShipmentItineraries");
        }
    }
}
