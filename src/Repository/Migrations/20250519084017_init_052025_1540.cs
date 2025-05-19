using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_1540 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Shipments_ShipmentId",
                table: "ShipmentTrackings");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "1e3dd8ba-637f-48b8-b5c0-64e52a22d74b");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "7084aa92-c7c1-4f90-9a09-8a881ab756f5");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "da54d233-9bb5-4f76-9bfa-fa4c70f6338a");

            migrationBuilder.RenameColumn(
                name: "ShipmentId",
                table: "ShipmentTrackings",
                newName: "ParcelId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_ShipmentId",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_ParcelId");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Parcels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParcelStatus",
                table: "Parcels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "Parcels",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4b6a9817-8b7b-4363-9d88-14463f3c1a2d", "879fa96f-5f1e-4c1d-82be-28ca125682c7", "Customer", "CUSTOMER" },
                    { "6408927a-2925-4b75-ad95-3e6046ed10b7", "64c34b27-d226-41c9-ac21-9d24db5252e3", "Staff", "STAFF" },
                    { "9c076cbb-bb52-432d-95e1-7d73e2a4bc40", "35ba96e6-3dc6-4945-b8cc-7fbc6ac50bcd", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ShipmentTrackings",
                column: "StationId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Parcels_ParcelId",
                table: "ShipmentTrackings");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Stations_StationId",
                table: "ShipmentTrackings");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ShipmentTrackings");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "4b6a9817-8b7b-4363-9d88-14463f3c1a2d");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "6408927a-2925-4b75-ad95-3e6046ed10b7");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "9c076cbb-bb52-432d-95e1-7d73e2a4bc40");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ParcelStatus",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "Parcels");

            migrationBuilder.RenameColumn(
                name: "ParcelId",
                table: "ShipmentTrackings",
                newName: "ShipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_ParcelId",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_ShipmentId");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1e3dd8ba-637f-48b8-b5c0-64e52a22d74b", "6c18fb08-8c19-41b1-98a9-b5aedb18c01f", "Staff", "STAFF" },
                    { "7084aa92-c7c1-4f90-9a09-8a881ab756f5", "3b79e315-e465-4770-a6da-5222a0bf3ab5", "Admin", "ADMIN" },
                    { "da54d233-9bb5-4f76-9bfa-fa4c70f6338a", "6d6913a2-e5c8-48fb-a787-01cbc55d61af", "Customer", "CUSTOMER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentTrackings_Shipments_ShipmentId",
                table: "ShipmentTrackings",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
