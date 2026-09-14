using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2B_PRO.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTenantCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantCode",
                table: "companyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantCode",
                table: "companyName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
