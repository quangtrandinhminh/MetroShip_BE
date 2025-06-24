using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_062025_109 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OversizeSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "OverweightSurchangeFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "PriceDescriptionJSON",
                table: "Parcels");

            migrationBuilder.AddColumn<string>(
                name: "PriceStructureDescriptionJSON",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnApprovedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnCancelledAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnDeliveredAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnPickupAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnRequestedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeSlotId",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceFeeVnd",
                table: "Parcels",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeVnd",
                table: "Parcels",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceStructureDescriptionJSON",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnApprovedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnCancelledAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnDeliveredAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnPickupAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnRequestedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TimeSlotId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingFeeVnd",
                table: "Parcels");

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceFeeVnd",
                table: "Parcels",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "PriceDescriptionJSON",
                table: "Parcels",
                type: "text",
                nullable: true);
        }
    }
}
