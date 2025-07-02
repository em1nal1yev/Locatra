using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocatraMain.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPaidToAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Auctions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Auctions");
        }
    }
}
