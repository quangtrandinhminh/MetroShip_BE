using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_062025_033 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "ReturnApprovedAt",
                table: "Shipments",
                newName: "ReturnConfirmedAt");

            migrationBuilder.RenameColumn(
                name: "PickupAt",
                table: "Shipments",
                newName: "PickedUpAt");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefreshTokenExpiredTime",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<string>(
                name: "RecipientNationalId",
                table: "Shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AwaitedDeliveryAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedBy",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveredPicture",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedUpPicture",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Rating",
                table: "Shipments",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnConfirmedBy",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "Shipments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParcelTrackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParcelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TrackingForShipmentStatus = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ParcelTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelTrackings_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelTrackings");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiredTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AwaitedDeliveryAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ConfirmedBy",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveredPicture",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickedUpPicture",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnConfirmedBy",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "Shipments");

            migrationBuilder.RenameColumn(
                name: "ReturnConfirmedAt",
                table: "Shipments",
                newName: "ReturnApprovedAt");

            migrationBuilder.RenameColumn(
                name: "PickedUpAt",
                table: "Shipments",
                newName: "PickupAt");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientNationalId",
                table: "Shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreatedAt",
                table: "RefreshTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Id",
                table: "RefreshTokens",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }
    }
}
