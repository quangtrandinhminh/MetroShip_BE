using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_2345 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetroTimeSlots_MetroLines_MetroLineId",
                table: "MetroTimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentTrackings_Stations_StationId",
                table: "ShipmentTrackings");

            migrationBuilder.DropTable(
                name: "MetroSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ShipmentTrackings");

            migrationBuilder.DropIndex(
                name: "IX_MetroTimeSlots_MetroLineId",
                table: "MetroTimeSlots");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "0bfab0b7-75e9-4024-b5c7-18c3478f61a8");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "ab5b94b3-ec6a-4a9b-8097-8d55f51e29b7");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "e7c3bf55-d9a7-44db-98cc-0ba0a136e7a5");

            migrationBuilder.DropColumn(
                name: "BookingAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TimeSlot",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "MetroLineId",
                table: "MetroTimeSlots");

            migrationBuilder.RenameIndex(
                name: "Index_Id14",
                table: "SystemConfigs",
                newName: "IX_SystemConfigs_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt14",
                table: "SystemConfigs",
                newName: "IX_SystemConfigs_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id13",
                table: "SupportingTickets",
                newName: "IX_SupportingTickets_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt13",
                table: "SupportingTickets",
                newName: "IX_SupportingTickets_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id12",
                table: "Stations",
                newName: "IX_Stations_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt12",
                table: "Stations",
                newName: "IX_Stations_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id11",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt11",
                table: "ShipmentTrackings",
                newName: "IX_ShipmentTrackings_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "Shipments",
                newName: "TotalCostVnd");

            migrationBuilder.RenameColumn(
                name: "ScheduledDate",
                table: "Shipments",
                newName: "SurchargeAppliedAt");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Shipments",
                newName: "ScheduledDateTime");

            migrationBuilder.RenameColumn(
                name: "PaymentAmount",
                table: "Shipments",
                newName: "SurchargeFeeVnd");

            migrationBuilder.RenameColumn(
                name: "InsuranceAmount",
                table: "Shipments",
                newName: "InsuranceFeeVnd");

            migrationBuilder.RenameColumn(
                name: "ArrivalStationId",
                table: "Shipments",
                newName: "DestinationStationId");

            migrationBuilder.RenameIndex(
                name: "Index_Id9",
                table: "Shipments",
                newName: "IX_Shipments_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt9",
                table: "Shipments",
                newName: "IX_Shipments_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id10",
                table: "ShipmentItineraries",
                newName: "IX_ShipmentItineraries_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt10",
                table: "ShipmentItineraries",
                newName: "IX_ShipmentItineraries_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id8",
                table: "Routes",
                newName: "IX_Routes_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt8",
                table: "Routes",
                newName: "IX_Routes_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id7",
                table: "Reports",
                newName: "IX_Reports_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt7",
                table: "Reports",
                newName: "IX_Reports_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id6",
                table: "Regions",
                newName: "IX_Regions_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt6",
                table: "Regions",
                newName: "IX_Regions_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id4",
                table: "Parcels",
                newName: "IX_Parcels_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt4",
                table: "Parcels",
                newName: "IX_Parcels_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id5",
                table: "ParcelCategories",
                newName: "IX_ParcelCategories_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt5",
                table: "ParcelCategories",
                newName: "IX_ParcelCategories_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id3",
                table: "Notifications",
                newName: "IX_Notifications_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt3",
                table: "Notifications",
                newName: "IX_Notifications_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id2",
                table: "MetroTimeSlots",
                newName: "IX_MetroTimeSlots_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt2",
                table: "MetroTimeSlots",
                newName: "IX_MetroTimeSlots_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "Index_Id1",
                table: "MetroLines",
                newName: "IX_MetroLines_Id");

            migrationBuilder.RenameIndex(
                name: "Index_CreatedAt1",
                table: "MetroLines",
                newName: "IX_MetroLines_CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ShipmentTrackings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BookedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PickupAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "Shipments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefundedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeVnd",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MetroBasePrices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeSlotId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BasePriceVndPerKm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_MetroBasePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetroBasePrices_MetroLines_LineId",
                        column: x => x.LineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetroBasePrices_MetroTimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "MetroTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PaidById = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymentTrackingId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentCurrency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PaymentTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_PaidById",
                        column: x => x.PaidById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1e3dd8ba-637f-48b8-b5c0-64e52a22d74b", "6c18fb08-8c19-41b1-98a9-b5aedb18c01f", "Staff", "STAFF" },
                    { "7084aa92-c7c1-4f90-9a09-8a881ab756f5", "3b79e315-e465-4770-a6da-5222a0bf3ab5", "Admin", "ADMIN" },
                    { "da54d233-9bb5-4f76-9bfa-fa4c70f6338a", "6d6913a2-e5c8-48fb-a787-01cbc55d61af", "Customer", "CUSTOMER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetroBasePrices_CreatedAt",
                table: "MetroBasePrices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MetroBasePrices_Id",
                table: "MetroBasePrices",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetroBasePrices_LineId",
                table: "MetroBasePrices",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_MetroBasePrices_TimeSlotId",
                table: "MetroBasePrices",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Id",
                table: "Transactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaidById",
                table: "Transactions",
                column: "PaidById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetroBasePrices");

            migrationBuilder.DropTable(
                name: "Transactions");

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

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "BookedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickupAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingFeeVnd",
                table: "Shipments");

            migrationBuilder.RenameIndex(
                name: "IX_SystemConfigs_Id",
                table: "SystemConfigs",
                newName: "Index_Id14");

            migrationBuilder.RenameIndex(
                name: "IX_SystemConfigs_CreatedAt",
                table: "SystemConfigs",
                newName: "Index_CreatedAt14");

            migrationBuilder.RenameIndex(
                name: "IX_SupportingTickets_Id",
                table: "SupportingTickets",
                newName: "Index_Id13");

            migrationBuilder.RenameIndex(
                name: "IX_SupportingTickets_CreatedAt",
                table: "SupportingTickets",
                newName: "Index_CreatedAt13");

            migrationBuilder.RenameIndex(
                name: "IX_Stations_Id",
                table: "Stations",
                newName: "Index_Id12");

            migrationBuilder.RenameIndex(
                name: "IX_Stations_CreatedAt",
                table: "Stations",
                newName: "Index_CreatedAt12");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_Id",
                table: "ShipmentTrackings",
                newName: "Index_Id11");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentTrackings_CreatedAt",
                table: "ShipmentTrackings",
                newName: "Index_CreatedAt11");

            migrationBuilder.RenameColumn(
                name: "TotalCostVnd",
                table: "Shipments",
                newName: "TotalCost");

            migrationBuilder.RenameColumn(
                name: "SurchargeFeeVnd",
                table: "Shipments",
                newName: "PaymentAmount");

            migrationBuilder.RenameColumn(
                name: "SurchargeAppliedAt",
                table: "Shipments",
                newName: "ScheduledDate");

            migrationBuilder.RenameColumn(
                name: "ScheduledDateTime",
                table: "Shipments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "InsuranceFeeVnd",
                table: "Shipments",
                newName: "InsuranceAmount");

            migrationBuilder.RenameColumn(
                name: "DestinationStationId",
                table: "Shipments",
                newName: "ArrivalStationId");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_Id",
                table: "Shipments",
                newName: "Index_Id9");

            migrationBuilder.RenameIndex(
                name: "IX_Shipments_CreatedAt",
                table: "Shipments",
                newName: "Index_CreatedAt9");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentItineraries_Id",
                table: "ShipmentItineraries",
                newName: "Index_Id10");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentItineraries_CreatedAt",
                table: "ShipmentItineraries",
                newName: "Index_CreatedAt10");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_Id",
                table: "Routes",
                newName: "Index_Id8");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_CreatedAt",
                table: "Routes",
                newName: "Index_CreatedAt8");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_Id",
                table: "Reports",
                newName: "Index_Id7");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_CreatedAt",
                table: "Reports",
                newName: "Index_CreatedAt7");

            migrationBuilder.RenameIndex(
                name: "IX_Regions_Id",
                table: "Regions",
                newName: "Index_Id6");

            migrationBuilder.RenameIndex(
                name: "IX_Regions_CreatedAt",
                table: "Regions",
                newName: "Index_CreatedAt6");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Id",
                table: "RefreshTokens",
                newName: "Index_Id");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_CreatedAt",
                table: "RefreshTokens",
                newName: "Index_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Parcels_Id",
                table: "Parcels",
                newName: "Index_Id4");

            migrationBuilder.RenameIndex(
                name: "IX_Parcels_CreatedAt",
                table: "Parcels",
                newName: "Index_CreatedAt4");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelCategories_Id",
                table: "ParcelCategories",
                newName: "Index_Id5");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelCategories_CreatedAt",
                table: "ParcelCategories",
                newName: "Index_CreatedAt5");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_Id",
                table: "Notifications",
                newName: "Index_Id3");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                newName: "Index_CreatedAt3");

            migrationBuilder.RenameIndex(
                name: "IX_MetroTimeSlots_Id",
                table: "MetroTimeSlots",
                newName: "Index_Id2");

            migrationBuilder.RenameIndex(
                name: "IX_MetroTimeSlots_CreatedAt",
                table: "MetroTimeSlots",
                newName: "Index_CreatedAt2");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_Id",
                table: "MetroLines",
                newName: "Index_Id1");

            migrationBuilder.RenameIndex(
                name: "IX_MetroLines_CreatedAt",
                table: "MetroLines",
                newName: "Index_CreatedAt1");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ShipmentTrackings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BookingAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PickupTime",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "TimeSlot",
                table: "Shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetroLineId",
                table: "MetroTimeSlots",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetroSchedules",
                columns: table => new
                {
                    LineId = table.Column<string>(type: "text", nullable: false),
                    TimeSlotId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetroSchedules", x => new { x.LineId, x.TimeSlotId });
                    table.ForeignKey(
                        name: "FK_MetroSchedules_MetroLines_LineId",
                        column: x => x.LineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetroSchedules_MetroTimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "MetroTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0bfab0b7-75e9-4024-b5c7-18c3478f61a8", "5c86df02-195b-41f3-86b5-fae2562d7fc6", "Staff", "STAFF" },
                    { "ab5b94b3-ec6a-4a9b-8097-8d55f51e29b7", "29cced99-ac0e-4d34-b4f8-361de4ef0ef5", "Customer", "CUSTOMER" },
                    { "e7c3bf55-d9a7-44db-98cc-0ba0a136e7a5", "3871ddf7-da32-40c4-b356-57a874a32258", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ShipmentTrackings",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_MetroTimeSlots_MetroLineId",
                table: "MetroTimeSlots",
                column: "MetroLineId");

            migrationBuilder.CreateIndex(
                name: "IX_MetroSchedules_TimeSlotId",
                table: "MetroSchedules",
                column: "TimeSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetroTimeSlots_MetroLines_MetroLineId",
                table: "MetroTimeSlots",
                column: "MetroLineId",
                principalTable: "MetroLines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentTrackings_Stations_StationId",
                table: "ShipmentTrackings",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }
    }
}
