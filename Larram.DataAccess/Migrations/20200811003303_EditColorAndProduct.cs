using Microsoft.EntityFrameworkCore.Migrations;

namespace Larram.DataAccess.Migrations
{
    public partial class EditColorAndProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HexValue",
                table: "Colors");

            migrationBuilder.AddColumn<string>(
                name: "HexValue",
                table: "Products",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HexValue",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "HexValue",
                table: "Colors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
