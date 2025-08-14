using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportingTickets");

            migrationBuilder.RenameColumn(
                name: "MinInsuranceRateOnValue",
                table: "InsurancePolicy",
                newName: "MinCompensationRateOnValue");

            migrationBuilder.RenameColumn(
                name: "MaxInsuranceRateOnValue",
                table: "InsurancePolicy",
                newName: "MaxCompensationRateOnValue");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompensatedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiredAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCompensationFeeVnd",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompensationFeeVnd",
                table: "Parcels",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCompensationRateOnShippingFee",
                table: "InsurancePolicy",
                type: "numeric(8,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OpenById = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResolvedById = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ResolvedContent = table.Column<string>(type: "text", nullable: true),
                    SupportType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_OpenById",
                        column: x => x.OpenById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_CreatedAt",
                table: "SupportTicket",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_Id",
                table: "SupportTicket",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_OpenById",
                table: "SupportTicket",
                column: "OpenById");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_ResolvedById",
                table: "SupportTicket",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_ShipmentId",
                table: "SupportTicket",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportTicket");

            migrationBuilder.DropColumn(
                name: "CompensatedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalCompensationFeeVnd",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CompensationFeeVnd",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "MaxCompensationRateOnShippingFee",
                table: "InsurancePolicy");

            migrationBuilder.RenameColumn(
                name: "MinCompensationRateOnValue",
                table: "InsurancePolicy",
                newName: "MinInsuranceRateOnValue");

            migrationBuilder.RenameColumn(
                name: "MaxCompensationRateOnValue",
                table: "InsurancePolicy",
                newName: "MaxInsuranceRateOnValue");

            migrationBuilder.CreateTable(
                name: "SupportingTickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportingTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportingTickets_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportingTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_CreatedAt",
                table: "SupportingTickets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_Id",
                table: "SupportingTickets",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_ShipmentId",
                table: "SupportingTickets",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_UserId",
                table: "SupportingTickets",
                column: "UserId");
        }
    }
}
