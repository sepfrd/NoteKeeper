#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeGoogleTokenEntityRefreshTokenNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 3, 21, 45, 13, 442, DateTimeKind.Utc).AddTicks(3930), "$2a$12$F3pXY7aGvJITrIrVBn4J9eFvEdHArmfO1I/GJM8MUMU2F/qsZkY2W", new DateTime(2024, 10, 3, 21, 45, 13, 442, DateTimeKind.Utc).AddTicks(3930), new Guid("33e86bdd-2eeb-4915-a3d8-4b0d088025c6") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 3, 21, 33, 6, 434, DateTimeKind.Utc).AddTicks(4580), "$2a$12$267vhvBaDwf4MwD9GE/pP.uTk0iyBZd0FrNkFoi8fLIH/PvfmzJGG", new DateTime(2024, 10, 3, 21, 33, 6, 434, DateTimeKind.Utc).AddTicks(4580), new Guid("260b2b26-061e-413d-92e4-baeb622dee48") });
        }
    }
}
