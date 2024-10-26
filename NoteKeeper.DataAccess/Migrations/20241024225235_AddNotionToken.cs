#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNotionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotionTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "VarChar", maxLength: 5000, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenType = table.Column<string>(type: "VarChar", maxLength: 100, nullable: false),
                    BotId = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    WorkspaceName = table.Column<string>(type: "VarChar", maxLength: 150, nullable: true),
                    WorkspaceIconUrl = table.Column<string>(type: "VarChar", maxLength: 250, nullable: true),
                    WorkspaceId = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    NotionId = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "VarChar", maxLength: 250, nullable: true),
                    NotionEmail = table.Column<string>(type: "VarChar", maxLength: 320, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotionTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotionTokens_Users_UserId",
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
                values: new object[] { new DateTime(2024, 10, 24, 22, 52, 35, 89, DateTimeKind.Utc).AddTicks(9120), "$2a$12$oxXEzD67D4jHjPIU7qj7QusvEgDsXYqYSCsSHGMEBRXM4uskMV.5W", new DateTime(2024, 10, 24, 22, 52, 35, 89, DateTimeKind.Utc).AddTicks(9120), new Guid("ed39a117-77d4-494c-8aaa-44e0739468d9") });

            migrationBuilder.CreateIndex(
                name: "IX_NotionTokens_UserId",
                table: "NotionTokens",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotionTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt", "Uuid" },
                values: new object[] { new DateTime(2024, 10, 11, 19, 29, 51, 126, DateTimeKind.Utc).AddTicks(920), "$2a$12$0/x6Lh4lFLtkny8JibDYDuzaKcs9Uuc4NQwEHcFCe.BcJpI1VjrHm", new DateTime(2024, 10, 11, 19, 29, 51, 126, DateTimeKind.Utc).AddTicks(920), new Guid("63130182-286c-4af5-afc5-3c45a470c54b") });
        }
    }
}
