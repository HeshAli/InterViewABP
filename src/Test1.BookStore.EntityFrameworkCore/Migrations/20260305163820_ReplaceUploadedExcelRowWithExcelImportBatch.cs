using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test1.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUploadedExcelRowWithExcelImportBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUploadedExcelRows");

            migrationBuilder.CreateTable(
                name: "AppExcelImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExcelImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppExcelDataRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColumnA = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ColumnB = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ColumnC = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExcelDataRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExcelDataRows_AppExcelImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "AppExcelImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExcelDataRows_BatchId",
                table: "AppExcelDataRows",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExcelDataRows_UploadedByUserId",
                table: "AppExcelDataRows",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExcelImportBatches_UploadedByUserId",
                table: "AppExcelImportBatches",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExcelImportBatches_UploadTimeUtc",
                table: "AppExcelImportBatches",
                column: "UploadTimeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExcelDataRows");

            migrationBuilder.DropTable(
                name: "AppExcelImportBatches");

            migrationBuilder.CreateTable(
                name: "AppUploadedExcelRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SheetName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    RowDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUploadedExcelRows", x => x.Id);
                });
        }
    }
}