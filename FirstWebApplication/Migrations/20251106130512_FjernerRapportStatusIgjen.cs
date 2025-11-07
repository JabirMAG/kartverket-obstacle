using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    public partial class FjernerRapportStatusIgjen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RapportStatus",
                table: "Rapports");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RapportStatus",
                table: "Rapports",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }
    }
}
