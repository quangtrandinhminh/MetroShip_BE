using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_102025_1610 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainId",
                table: "StaffAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_TrainId",
                table: "StaffAssignments",
                column: "TrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAssignments_MetroTrains_TrainId",
                table: "StaffAssignments",
                column: "TrainId",
                principalTable: "MetroTrains",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffAssignments_MetroTrains_TrainId",
                table: "StaffAssignments");

            migrationBuilder.DropIndex(
                name: "IX_StaffAssignments_TrainId",
                table: "StaffAssignments");

            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "StaffAssignments");
        }
    }
}
