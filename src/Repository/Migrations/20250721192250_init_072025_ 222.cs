using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_072025_222 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes");

            migrationBuilder.RenameColumn(
                name: "ShipmentMediaType",
                table: "ShipmentImages",
                newName: "BusinessMediaType");

            migrationBuilder.RenameColumn(
                name: "StationCoordinateList",
                table: "MetroLines",
                newName: "StationListJSON");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ToTime",
                table: "StaffAssignments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "FromTime",
                table: "StaffAssignments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "PricingConfigId",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParcelMedia",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParcelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MediaUrl = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BusinessMediaType = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ParcelMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelMedia_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingConfig",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistanceTier",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PricingConfigId = table.Column<string>(type: "text", nullable: false),
                    TierOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxDistanceKm = table.Column<decimal>(type: "numeric", nullable: true),
                    BasePriceVnd = table.Column<decimal>(type: "numeric", nullable: false),
                    BasePriceVndPerKm = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPricePerKm = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistanceTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistanceTier_PricingConfig_PricingConfigId",
                        column: x => x.PricingConfigId,
                        principalTable: "PricingConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeightTier",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PricingConfigId = table.Column<string>(type: "text", nullable: false),
                    TierOrder = table.Column<int>(type: "integer", nullable: false),
                    MinWeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxWeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    BasePriceVndPerKmPerKg = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightTier_PricingConfig_PricingConfigId",
                        column: x => x.PricingConfigId,
                        principalTable: "PricingConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PricingConfigId",
                table: "Shipments",
                column: "PricingConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes",
                columns: new[] { "LineId", "FromStationId", "ToStationId" });

            migrationBuilder.CreateIndex(
                name: "IX_DistanceTier_CreatedAt",
                table: "DistanceTier",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DistanceTier_Id",
                table: "DistanceTier",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistanceTier_PricingConfigId",
                table: "DistanceTier",
                column: "PricingConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelMedia_CreatedAt",
                table: "ParcelMedia",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelMedia_Id",
                table: "ParcelMedia",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelMedia_ParcelId",
                table: "ParcelMedia",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingConfig_CreatedAt",
                table: "PricingConfig",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PricingConfig_Id",
                table: "PricingConfig",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightTier_CreatedAt",
                table: "WeightTier",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WeightTier_Id",
                table: "WeightTier",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightTier_PricingConfigId",
                table: "WeightTier",
                column: "PricingConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_PricingConfig_PricingConfigId",
                table: "Shipments",
                column: "PricingConfigId",
                principalTable: "PricingConfig",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_PricingConfig_PricingConfigId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "DistanceTier");

            migrationBuilder.DropTable(
                name: "ParcelMedia");

            migrationBuilder.DropTable(
                name: "WeightTier");

            migrationBuilder.DropTable(
                name: "PricingConfig");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PricingConfigId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "PricingConfigId",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "BusinessMediaType",
                table: "ShipmentImages",
                newName: "ShipmentMediaType");

            migrationBuilder.RenameColumn(
                name: "StationListJSON",
                table: "MetroLines",
                newName: "StationCoordinateList");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ToTime",
                table: "StaffAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "FromTime",
                table: "StaffAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_LineId_FromStationId_ToStationId",
                table: "Routes",
                columns: new[] { "LineId", "FromStationId", "ToStationId" },
                unique: true);
        }
    }
}
