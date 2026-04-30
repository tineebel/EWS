using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgUnitShortCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectShortCode",
                schema: "dbo",
                table: "Sections",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeptShortCode",
                schema: "dbo",
                table: "Departments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [dbo].[Departments]
                SET [DeptShortCode] = [DeptCode]
                WHERE [DeptShortCode] IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE [dbo].[Sections]
                SET [SectShortCode] = [SectCode]
                WHERE [SectShortCode] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectShortCode",
                schema: "dbo",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "DeptShortCode",
                schema: "dbo",
                table: "Departments");
        }
    }
}
