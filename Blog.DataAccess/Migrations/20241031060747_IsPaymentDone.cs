using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IsPaymentDone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentDone",
                table: "Order");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentDone",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentDone",
                table: "OrderItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentDone",
                table: "Order",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
