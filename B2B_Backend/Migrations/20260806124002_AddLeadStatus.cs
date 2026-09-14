using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2B_PRO.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "leadsDatas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "leadsDatas");
        }
    }
}
