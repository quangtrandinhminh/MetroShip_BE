using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_050 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalRefundedFeeVnd",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRefundedFeeVnd",
                table: "Shipments");
        }
    }
}
