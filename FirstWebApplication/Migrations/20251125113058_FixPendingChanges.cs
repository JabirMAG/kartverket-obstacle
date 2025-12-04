using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ObstaclesData_OwnerUserId",
                table: "ObstaclesData",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObstaclesData_AspNetUsers_OwnerUserId",
                table: "ObstaclesData",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObstaclesData_AspNetUsers_OwnerUserId",
                table: "ObstaclesData");

            migrationBuilder.DropIndex(
                name: "IX_ObstaclesData_OwnerUserId",
                table: "ObstaclesData");
        }
    }
}
