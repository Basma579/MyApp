using Microsoft.EntityFrameworkCore.Migrations;

namespace MyApp.Migrations
{
    public partial class AddItemName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "order_Details",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "order_Details");
        }
    }
}
