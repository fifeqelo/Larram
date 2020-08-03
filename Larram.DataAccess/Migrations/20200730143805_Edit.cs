using Microsoft.EntityFrameworkCore.Migrations;

namespace Larram.DataAccess.Migrations
{
    public partial class Edit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "ProductAvailabilities");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ProductAvailabilities");

            migrationBuilder.AddColumn<double>(
                name: "DiscountPrice",
                table: "Products",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Products",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.AddColumn<double>(
                name: "DiscountPrice",
                table: "ProductAvailabilities",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "ProductAvailabilities",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
