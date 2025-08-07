using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_082025_1506 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "Transactions");
        }
    }
}
