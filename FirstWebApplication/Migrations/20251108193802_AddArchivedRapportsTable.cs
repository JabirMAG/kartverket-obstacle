using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedRapportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedRapports");
        }
    }
}
