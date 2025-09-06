using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_092025_329 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxCompensationRateOnShippingFee",
                table: "InsurancePolicy",
                newName: "MinCompensationRateOnShippingFee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinCompensationRateOnShippingFee",
                table: "InsurancePolicy",
                newName: "MaxCompensationRateOnShippingFee");
        }
    }
}
