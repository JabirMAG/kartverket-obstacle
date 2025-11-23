using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class CombineArchivedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RapportComments column only if it doesn't exist (for backward compatibility)
            // If InitialCreate already created it, this will be skipped
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'ArchivedReports' 
                    AND COLUMN_NAME = 'RapportComments'
                );
                SET @sql = IF(@col_exists = 0, 
                    CONCAT('ALTER TABLE `ArchivedReports` ADD `RapportComments` longtext CHARACTER SET utf8mb4 NOT NULL DEFAULT (', CHAR(39), '[]', CHAR(39), ')'),
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Migrate existing data from ArchivedRapports to ArchivedReports
            // This will only work if ArchivedRapports table exists
            migrationBuilder.Sql(@"
                UPDATE ArchivedReports ar
                SET RapportComments = COALESCE((
                    SELECT CONCAT('[', GROUP_CONCAT(
                        CONCAT('""', REPLACE(RapportComment, '""', '\\""'), '""')
                        SEPARATOR ','
                    ), ']')
                    FROM ArchivedRapports arr
                    WHERE arr.ArchivedReportId = ar.ArchivedReportId
                ), '[]')
                WHERE EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'ArchivedRapports'
                ) AND EXISTS (
                    SELECT 1 FROM ArchivedRapports arr 
                    WHERE arr.ArchivedReportId = ar.ArchivedReportId
                )
            ");

            // Drop the ArchivedRapports table if it exists
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS `ArchivedRapports`;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RapportComments",
                table: "ArchivedReports");

            migrationBuilder.CreateTable(
                name: "ArchivedRapports",
                columns: table => new
                {
                    ArchivedRapportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ArchivedReportId = table.Column<int>(type: "int", nullable: false),
                    OriginalRapportId = table.Column<int>(type: "int", nullable: false),
                    RapportComment = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedRapports", x => x.ArchivedRapportId);
                    table.ForeignKey(
                        name: "FK_ArchivedRapports_ArchivedReports_ArchivedReportId",
                        column: x => x.ArchivedReportId,
                        principalTable: "ArchivedReports",
                        principalColumn: "ArchivedReportId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedRapports_ArchivedReportId",
                table: "ArchivedRapports",
                column: "ArchivedReportId");
        }
    }
}
