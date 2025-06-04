using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_127 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "4b6a9817-8b7b-4363-9d88-14463f3c1a2d");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "6408927a-2925-4b75-ad95-3e6046ed10b7");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "9c076cbb-bb52-432d-95e1-7d73e2a4bc40");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4b6a9817-8b7b-4363-9d88-14463f3c1a2d", "879fa96f-5f1e-4c1d-82be-28ca125682c7", "Customer", "CUSTOMER" },
                    { "6408927a-2925-4b75-ad95-3e6046ed10b7", "64c34b27-d226-41c9-ac21-9d24db5252e3", "Staff", "STAFF" },
                    { "9c076cbb-bb52-432d-95e1-7d73e2a4bc40", "35ba96e6-3dc6-4945-b8cc-7fbc6ac50bcd", "Admin", "ADMIN" }
                });
        }
    }
}
