using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_20250522_1500 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Shipments_ShipmentId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ShipmentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShipmentId",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ShipmentId",
                table: "Transactions",
                column: "ShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Shipments_ShipmentId",
                table: "Transactions",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }
    }
}
