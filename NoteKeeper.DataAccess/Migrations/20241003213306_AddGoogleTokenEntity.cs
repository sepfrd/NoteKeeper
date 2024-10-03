using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "VarChar", maxLength: 5000, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefreshToken = table.Column<string>(type: "VarChar", maxLength: 1000, nullable: false),
                    Scope = table.Column<string>(type: "VarChar", maxLength: 1000, nullable: false),
                    TokenType = table.Column<string>(type: "VarChar", maxLength: 100, nullable: false),
                    IdToken = table.Column<string>(type: "VarChar", maxLength: 5000, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 3, 21, 33, 6, 434, DateTimeKind.Utc).AddTicks(4580), "$2a$12$267vhvBaDwf4MwD9GE/pP.uTk0iyBZd0FrNkFoi8fLIH/PvfmzJGG", new DateTime(2024, 10, 3, 21, 33, 6, 434, DateTimeKind.Utc).AddTicks(4580), new Guid("260b2b26-061e-413d-92e4-baeb622dee48") });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 9, 21, 9, 52, 47, 970, DateTimeKind.Utc).AddTicks(4180), "$2a$12$1N.q/VHYEAb348.aVzknrORT58Z/QRqgiI0elQ10BgLN7XmuDJBGi", new DateTime(2024, 9, 21, 9, 52, 47, 970, DateTimeKind.Utc).AddTicks(4180), new Guid("e51ebcab-0601-41fc-8ee8-2cfd8c805505") });
        }
    }
}
