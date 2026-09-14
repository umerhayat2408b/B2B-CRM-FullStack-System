using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2B_PRO.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "leadsDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_leadsDatas_CompanyId",
                table: "leadsDatas",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_leadsDatas_companyName_CompanyId",
                table: "leadsDatas",
                column: "CompanyId",
                principalTable: "companyName",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leadsDatas_companyName_CompanyId",
                table: "leadsDatas");

            migrationBuilder.DropIndex(
                name: "IX_leadsDatas_CompanyId",
                table: "leadsDatas");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "leadsDatas");
        }
    }
}
