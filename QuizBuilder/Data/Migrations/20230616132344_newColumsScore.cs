using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizBuilder.Data.Migrations
{
    public partial class newColumsScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tests");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Questions");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Tests",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
