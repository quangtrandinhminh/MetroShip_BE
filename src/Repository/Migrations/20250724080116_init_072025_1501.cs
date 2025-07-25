using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_072025_1501 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceVnd",
                table: "WeightTier",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPricePerKmAndKg",
                table: "WeightTier",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StationCodeListJSON",
                table: "Stations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "MetroLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePriceVndPerKm",
                table: "DistanceTier",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePriceVnd",
                table: "DistanceTier",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePriceVnd",
                table: "WeightTier");

            migrationBuilder.DropColumn(
                name: "IsPricePerKmAndKg",
                table: "WeightTier");

            migrationBuilder.DropColumn(
                name: "StationCodeListJSON",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "MetroLines");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePriceVndPerKm",
                table: "DistanceTier",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePriceVnd",
                table: "DistanceTier",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
