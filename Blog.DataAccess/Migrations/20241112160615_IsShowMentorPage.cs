using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IsShowMentorPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShowMenuPage",
                table: "Faculties",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShowMenuPage",
                table: "Faculties");
        }
    }
}
