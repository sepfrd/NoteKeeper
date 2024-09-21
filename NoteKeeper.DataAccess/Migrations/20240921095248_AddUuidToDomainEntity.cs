using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUuidToDomainEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 9, 21, 9, 52, 47, 970, DateTimeKind.Utc).AddTicks(4180), "$2a$12$1N.q/VHYEAb348.aVzknrORT58Z/QRqgiI0elQ10BgLN7XmuDJBGi", new DateTime(2024, 9, 21, 9, 52, 47, 970, DateTimeKind.Utc).AddTicks(4180), new Guid("e51ebcab-0601-41fc-8ee8-2cfd8c805505") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Notes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 9, 20, 23, 46, 42, 779, DateTimeKind.Utc).AddTicks(6650), "$2a$12$9FpbeHCX6SZA5DcxY4DMQu9V46tH9gVU0ZZTAu.Jyg/YunXt3ibT.", new DateTime(2024, 9, 20, 23, 46, 42, 779, DateTimeKind.Utc).AddTicks(6650) });
        }
    }
}
