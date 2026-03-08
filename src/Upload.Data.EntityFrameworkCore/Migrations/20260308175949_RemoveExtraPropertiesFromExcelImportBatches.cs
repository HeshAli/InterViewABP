using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Upload.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraPropertiesFromExcelImportBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppExcelImportBatches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppExcelImportBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
