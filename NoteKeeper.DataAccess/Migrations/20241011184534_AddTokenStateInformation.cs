#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenStateInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TokenStateId",
                table: "GoogleTokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TokenStateInformation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "VarChar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "VarChar", maxLength: 100, nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenStateInformation", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TokenStateInformation",
                columns: new[] { "Id", "CreatedAt", "Description", "Title", "UpdatedAt", "Uuid" },
                values: new object[,]
                {
                    { 1L, new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1060), "Neither access token nor refresh token has been revoked.", "Active", new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1060), new Guid("a0cf4d5f-ed6b-4de2-bfcb-01e14ddbd825") },
                    { 2L, new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1110), "Both tokens have been revoked.", "Deleted", new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1110), new Guid("7e004b48-e8e8-498d-886f-4b12cb3e0134") },
                    { 3L, new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1120), "Access token has been revoked.", "Access Token Revoked", new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1120), new Guid("0d448d91-52c6-49e1-853f-2618ae0dca7c") },
                    { 4L, new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1120), "Refresh token has been revoked.", "Refresh Token Revoked", new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1120), new Guid("3e0d5148-919f-4029-a179-c2c592c88894") }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1220), "$2a$12$SvvxrIjm9.UKcP/Qjl.c6efFh/RgIO5XRqYBP/QO01/AWsmFUlP4C", new DateTime(2024, 10, 11, 18, 45, 33, 577, DateTimeKind.Utc).AddTicks(1220), new Guid("f0a8a301-7075-44f9-8491-b036934d23f7") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenStateInformation");

            migrationBuilder.DropColumn(
                name: "TokenStateId",
                table: "GoogleTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 4, 12, 26, 57, 620, DateTimeKind.Utc).AddTicks(8580), "$2a$12$RcIf3WgA.K4RaB3xda0vgezs870kB2lUfelrlY0EkQMC8vSSkS/Dq", new DateTime(2024, 10, 4, 12, 26, 57, 620, DateTimeKind.Utc).AddTicks(8580), new Guid("983e7806-73d9-48a3-b9bc-b3108c585987") });
        }
    }
}
