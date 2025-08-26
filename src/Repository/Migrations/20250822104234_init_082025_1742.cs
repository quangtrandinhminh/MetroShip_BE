using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_1742 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "CutOffTime",
                table: "MetroTimeSlots",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartReceivingTime",
                table: "MetroTimeSlots",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CutOffTime",
                table: "MetroTimeSlots");

            migrationBuilder.DropColumn(
                name: "StartReceivingTime",
                table: "MetroTimeSlots");
        }
    }
}
