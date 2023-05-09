using Microsoft.EntityFrameworkCore.Migrations;

namespace ApplicationAuth.DAL.Migrations
{
    public partial class Alter_Bot_Dialogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InProccess",
                table: "Dialogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InProccess",
                table: "Dialogs");
        }
    }
}
