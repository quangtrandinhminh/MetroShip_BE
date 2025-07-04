using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_072025_312 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickedUpPicture",
                table: "Shipments",
                newName: "RejectionReason");

            migrationBuilder.RenameColumn(
                name: "DeliveredPicture",
                table: "Shipments",
                newName: "RejectedBy");

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentDealine",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedUpBy",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedUpImageLink",
                table: "Shipments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "PaymentDealine",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickedUpBy",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickedUpImageLink",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "RejectionReason",
                table: "Shipments",
                newName: "PickedUpPicture");

            migrationBuilder.RenameColumn(
                name: "RejectedBy",
                table: "Shipments",
                newName: "DeliveredPicture");
        }
    }
}
