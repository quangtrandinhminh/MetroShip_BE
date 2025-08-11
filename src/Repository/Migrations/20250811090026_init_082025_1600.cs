using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_1600 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_ParcelCategories_ParcelCategoryId",
                table: "Parcels");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_ParcelCategoryId",
                table: "Parcels");

            migrationBuilder.AddColumn<string>(
                name: "TrainScheduleId",
                table: "ShipmentItineraries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSurchargePerDayVnd",
                table: "PricingConfig",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PricingConfig",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FreeStoreDays",
                table: "PricingConfig",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefundForCancellationBeforeScheduledHours",
                table: "PricingConfig",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundRate",
                table: "PricingConfig",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentParcelStatus",
                table: "ParcelTrackings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ParcelCategoryId",
                table: "Parcels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "CategoryInsuranceId",
                table: "Parcels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Parcels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValueVnd",
                table: "ParcelCategories",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InsurancePolicy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BaseFeeVnd = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxParcelValueVnd = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InsuranceFeeRateOnValue = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    StandardCompensationValueVnd = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxInsuranceRateOnValue = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    MinInsuranceRateOnValue = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_InsurancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainSchedule",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrainId = table.Column<string>(type: "text", nullable: false),
                    TimeSlotId = table.Column<string>(type: "text", nullable: false),
                    LineId = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    DepartureStationId = table.Column<string>(type: "text", nullable: false),
                    DestinationStationId = table.Column<string>(type: "text", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    ArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainSchedule_MetroTimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "MetroTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainSchedule_MetroTrains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "MetroTrains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryInsurance",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParcelCategoryId = table.Column<string>(type: "text", nullable: false),
                    InsurancePolicyId = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_CategoryInsurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryInsurance_InsurancePolicy_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalTable: "InsurancePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryInsurance_ParcelCategories_ParcelCategoryId",
                        column: x => x.ParcelCategoryId,
                        principalTable: "ParcelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_CategoryInsuranceId",
                table: "Parcels",
                column: "CategoryInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryInsurance_CreatedAt",
                table: "CategoryInsurance",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryInsurance_Id",
                table: "CategoryInsurance",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryInsurance_InsurancePolicyId",
                table: "CategoryInsurance",
                column: "InsurancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryInsurance_ParcelCategoryId",
                table: "CategoryInsurance",
                column: "ParcelCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicy_CreatedAt",
                table: "InsurancePolicy",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicy_Id",
                table: "InsurancePolicy",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedule_CreatedAt",
                table: "TrainSchedule",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedule_Id",
                table: "TrainSchedule",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedule_TimeSlotId",
                table: "TrainSchedule",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedule_TrainId",
                table: "TrainSchedule",
                column: "TrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_CategoryInsurance_CategoryInsuranceId",
                table: "Parcels",
                column: "CategoryInsuranceId",
                principalTable: "CategoryInsurance",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_CategoryInsurance_CategoryInsuranceId",
                table: "Parcels");

            migrationBuilder.DropTable(
                name: "CategoryInsurance");

            migrationBuilder.DropTable(
                name: "TrainSchedule");

            migrationBuilder.DropTable(
                name: "InsurancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_CategoryInsuranceId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "TrainScheduleId",
                table: "ShipmentItineraries");

            migrationBuilder.DropColumn(
                name: "BaseSurchargePerDayVnd",
                table: "PricingConfig");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PricingConfig");

            migrationBuilder.DropColumn(
                name: "FreeStoreDays",
                table: "PricingConfig");

            migrationBuilder.DropColumn(
                name: "RefundForCancellationBeforeScheduledHours",
                table: "PricingConfig");

            migrationBuilder.DropColumn(
                name: "RefundRate",
                table: "PricingConfig");

            migrationBuilder.DropColumn(
                name: "CurrentParcelStatus",
                table: "ParcelTrackings");

            migrationBuilder.DropColumn(
                name: "CategoryInsuranceId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "MinValueVnd",
                table: "ParcelCategories");

            migrationBuilder.AlterColumn<string>(
                name: "ParcelCategoryId",
                table: "Parcels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_ParcelCategoryId",
                table: "Parcels",
                column: "ParcelCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_ParcelCategories_ParcelCategoryId",
                table: "Parcels",
                column: "ParcelCategoryId",
                principalTable: "ParcelCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
