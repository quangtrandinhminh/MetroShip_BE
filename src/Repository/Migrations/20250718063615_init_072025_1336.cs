using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_072025_1336 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredImageLink",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "NationalIdImageBackLink",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "NationalIdImageFrontLink",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickedUpImageLink",
                table: "Shipments");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionImageUrl",
                table: "Parcels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShipmentImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MediaUrl = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ShipmentMediaType = table.Column<int>(type: "integer", nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentImages_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    StaffId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedRole = table.Column<int>(type: "integer", nullable: false),
                    FromTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ToTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAssignments_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAssignments_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentImages_CreatedAt",
                table: "ShipmentImages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentImages_Id",
                table: "ShipmentImages",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentImages_ShipmentId",
                table: "ShipmentImages",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_CreatedAt",
                table: "StaffAssignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_Id",
                table: "StaffAssignments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_StaffId",
                table: "StaffAssignments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_StationId",
                table: "StaffAssignments",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentImages");

            migrationBuilder.DropTable(
                name: "StaffAssignments");

            migrationBuilder.DropColumn(
                name: "DescriptionImageUrl",
                table: "Parcels");

            migrationBuilder.AddColumn<string>(
                name: "DeliveredImageLink",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdImageBackLink",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdImageFrontLink",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedUpImageLink",
                table: "Shipments",
                type: "text",
                nullable: true);
        }
    }
}
