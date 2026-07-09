using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maham.Infrastructure.Migrations
{
    public partial class AddProjectMemberStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProjectMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProjectMembers");
        }
    }
}
