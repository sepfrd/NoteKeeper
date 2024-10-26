#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VarChar",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationType",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "RegistrationType", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 26, 20, 29, 13, 228, DateTimeKind.Utc).AddTicks(7540), "$2a$12$qUVh4Gt2tH5kbtPjrfcR.efYZz48rXwUA7TZRCqaxfJW8s2d39Qp.", 0, new DateTime(2024, 10, 26, 20, 29, 13, 228, DateTimeKind.Utc).AddTicks(7540), new Guid("0f29dba4-9bfd-4495-a282-be7075902626") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationType",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VarChar",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 24, 22, 52, 35, 89, DateTimeKind.Utc).AddTicks(9120), "$2a$12$oxXEzD67D4jHjPIU7qj7QusvEgDsXYqYSCsSHGMEBRXM4uskMV.5W", new DateTime(2024, 10, 24, 22, 52, 35, 89, DateTimeKind.Utc).AddTicks(9120), new Guid("ed39a117-77d4-494c-8aaa-44e0739468d9") });
        }
    }
}
