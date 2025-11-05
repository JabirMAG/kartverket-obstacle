using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FirstWebApplication.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class InitialAuthSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "27a609d2-154c-41bb-8257-45314e8065f2", "d01a810e-9587-4732-90dd-208175e61b60" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2de8d9c9-988c-400b-ac7d-7b45c59b6251", "d01a810e-9587-4732-90dd-208175e61b60" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "b4b5065b-e9dc-40d4-a49d-f00d9c720e75", "d01a810e-9587-4732-90dd-208175e61b60" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "27a609d2-154c-41bb-8257-45314e8065f2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2de8d9c9-988c-400b-ac7d-7b45c59b6251");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b4b5065b-e9dc-40d4-a49d-f00d9c720e75");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d01a810e-9587-4732-90dd-208175e61b60");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "27a609d2-154c-41bb-8257-45314e8065f2", "27a609d2-154c-41bb-8257-45314e8065f2", "Registerfører", "Registerfører" },
                    { "2de8d9c9-988c-400b-ac7d-7b45c59b6251", "2de8d9c9-988c-400b-ac7d-7b45c59b6251", "Admin", "Admin" },
                    { "b4b5065b-e9dc-40d4-a49d-f00d9c720e75", "b4b5065b-e9dc-40d4-a49d-f00d9c720e75", "Pilot", "Pilot" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d01a810e-9587-4732-90dd-208175e61b60", 0, "951f2472-d989-4f10-8f40-2e6cca5e7fd3", "admin@kartverket.com", false, null, false, null, "ADMIN@KARTVERKET.NO", "ADMIN@KARTVERKET.NO", "AQAAAAIAAYagAAAAEOBovHKIDrXLSmsxsdjlpuO3xSsmEqRi9aM680XUZiCgKWV7d0oiiTZdZb7WSJWHgg==", null, false, "996e3c4c-3da0-4c92-ade0-1b1a49266754", false, "admin@kartverket.no" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "27a609d2-154c-41bb-8257-45314e8065f2", "d01a810e-9587-4732-90dd-208175e61b60" },
                    { "2de8d9c9-988c-400b-ac7d-7b45c59b6251", "d01a810e-9587-4732-90dd-208175e61b60" },
                    { "b4b5065b-e9dc-40d4-a49d-f00d9c720e75", "d01a810e-9587-4732-90dd-208175e61b60" }
                });
        }
    }
}
