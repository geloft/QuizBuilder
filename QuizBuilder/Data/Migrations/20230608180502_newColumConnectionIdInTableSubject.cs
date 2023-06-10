using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizBuilder.Data.Migrations
{
    public partial class newColumConnectionIdInTableSubject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Subjects");
        }
    }
}
