using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_062025_342 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetroBasePrices");

            migrationBuilder.DropTable(
                name: "ParcelTrackings");

            migrationBuilder.DropIndex(
                name: "IX_Stations_StationCode_RegionId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Routes_RouteCode",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "BasePriceVndPerKm",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "EstMinute",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "EstimatedArrival",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "IsBulk",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ParcelStatus",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "BasePriceVndPerKm",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "CarriageHeightMeter",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "CarriageLenghtMeter",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "CarriageWeightTons",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "CarriageWidthMeter",
                table: "MetroLines");

            migrationBuilder.DropColumn(
                name: "CarriagesPerTrain",
                table: "MetroLines");

            migrationBuilder.RenameColumn(
                name: "SurchargeFeeVnd",
                table: "Shipments",
                newName: "TotalSurchargeFeeVnd");

            migrationBuilder.RenameColumn(
                name: "ShippingFeeVnd",
                table: "Shipments",
                newName: "TotalShippingFeeVnd");

            migrationBuilder.RenameColumn(
                name: "InsuranceFeeVnd",
                table: "Shipments",
                newName: "TotalOverdueSurchargeFee");

            migrationBuilder.RenameColumn(
                name: "EstimatedDeparture",
                table: "ShipmentItineraries",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "QrCode",
                table: "Parcels",
                newName: "PriceDescriptionJSON");

            migrationBuilder.RenameColumn(
                name: "IsBulk",
                table: "ParcelCategories",
                newName: "IsInsuranceRequired");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SystemConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "StationCode",
                table: "Stations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ReturnForShipmentId",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledShift",
                table: "Shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInsuranceFeeVnd",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVolumeM3",
                table: "Shipments",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeightKg",
                table: "Shipments",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ShipmentItineraries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeSlotId",
                table: "ShipmentItineraries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainId",
                table: "ShipmentItineraries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TravelTimeMin",
                table: "Routes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "RouteCode",
                table: "Routes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFeeVnd",
                table: "Parcels",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KmSurchangeFeeVnd",
                table: "Parcels",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverdueSurchangeFeeVnd",
                table: "Parcels",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OversizeSurchangeFeeVnd",
                table: "Parcels",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverweightSurchangeFeeVnd",
                table: "Parcels",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueVnd",
                table: "Parcels",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFeeVnd",
                table: "ParcelCategories",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceRate",
                table: "ParcelCategories",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSizeLimitCm",
                table: "ParcelCategories",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StationCoordinateList",
                table: "MetroLines",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetroTrains",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrainCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ModelName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CapacityPerCarriage = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MetroTrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetroTrains_MetroLines_LineId",
                        column: x => x.LineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ReturnForShipmentId",
                table: "Shipments",
                column: "ReturnForShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItineraries_TimeSlotId",
                table: "ShipmentItineraries",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItineraries_TrainId",
                table: "ShipmentItineraries",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_MetroTrains_CreatedAt",
                table: "MetroTrains",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MetroTrains_Id",
                table: "MetroTrains",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetroTrains_LineId",
                table: "MetroTrains",
                column: "LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItineraries_MetroTimeSlots_TimeSlotId",
                table: "ShipmentItineraries",
                column: "TimeSlotId",
                principalTable: "MetroTimeSlots",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItineraries_MetroTrains_TrainId",
                table: "ShipmentItineraries",
                column: "TrainId",
                principalTable: "MetroTrains",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Shipments_ReturnForShipmentId",
                table: "Shipments",
                column: "ReturnForShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItineraries_MetroTimeSlots_TimeSlotId",
                table: "ShipmentItineraries");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItineraries_MetroTrains_TrainId",
                table: "ShipmentItineraries");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Shipments_ReturnForShipmentId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "MetroTrains");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ReturnForShipmentId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItineraries_TimeSlotId",
                table: "ShipmentItineraries");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItineraries_TrainId",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "ReturnForShipmentId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ScheduledShift",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalInsuranceFeeVnd",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalVolumeM3",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "TimeSlotId",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "InsuranceFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "KmSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "OverdueSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "OversizeSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "OverweightSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ValueVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "InsuranceFeeVnd",
                table: "ParcelCategories");

            migrationBuilder.DropColumn(
                name: "InsuranceRate",
                table: "ParcelCategories");

            migrationBuilder.DropColumn(
                name: "TotalSizeLimitCm",
                table: "ParcelCategories");

            migrationBuilder.DropColumn(
                name: "StationCoordinateList",
                table: "MetroLines");

            migrationBuilder.RenameColumn(
                name: "TotalSurchargeFeeVnd",
                table: "Shipments",
                newName: "SurchargeFeeVnd");

            migrationBuilder.RenameColumn(
                name: "TotalShippingFeeVnd",
                table: "Shipments",
                newName: "ShippingFeeVnd");

            migrationBuilder.RenameColumn(
                name: "TotalOverdueSurchargeFee",
                table: "Shipments",
                newName: "InsuranceFeeVnd");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "ShipmentItineraries",
                newName: "EstimatedDeparture");

            migrationBuilder.RenameColumn(
                name: "PriceDescriptionJSON",
                table: "Parcels",
                newName: "QrCode");

            migrationBuilder.RenameColumn(
                name: "IsInsuranceRequired",
                table: "ParcelCategories",
                newName: "IsBulk");

            migrationBuilder.AlterColumn<string>(
                name: "StationCode",
                table: "Stations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceVndPerKm",
                table: "ShipmentItineraries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EstMinute",
                table: "ShipmentItineraries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EstimatedArrival",
                table: "ShipmentItineraries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TravelTimeMin",
                table: "Routes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RouteCode",
                table: "Routes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Parcels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBulk",
                table: "Parcels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParcelStatus",
                table: "Parcels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceVndPerKm",
                table: "MetroLines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageHeightMeter",
                table: "MetroLines",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageLenghtMeter",
                table: "MetroLines",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageWeightTons",
                table: "MetroLines",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarriageWidthMeter",
                table: "MetroLines",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarriagesPerTrain",
                table: "MetroLines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetroBasePrices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeSlotId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BasePriceVndPerKm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "ParcelTrackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParcelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelTrackings_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParcelTrackings_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationCode_RegionId",
                table: "Stations",
                columns: new[] { "StationCode", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_RouteCode",
                table: "Routes",
                column: "RouteCode",
                unique: true);

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
                name: "IX_ParcelTrackings_CreatedAt",
                table: "ParcelTrackings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelTrackings_Id",
                table: "ParcelTrackings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelTrackings_ParcelId",
                table: "ParcelTrackings",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelTrackings_StationId",
                table: "ParcelTrackings",
                column: "StationId");
        }
    }
}
