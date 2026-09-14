using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2B_PRO.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "companyName",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "companyName",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "companyname",
                table: "companyName",
                newName: "CompanyName");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "companyName",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TenantCode",
                table: "companyName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "companyName");

            migrationBuilder.DropColumn(
                name: "TenantCode",
                table: "companyName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "companyName",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "companyName",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "companyName",
                newName: "companyname");
        }
    }
}
