using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "DomainEntitySequence");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DomainEntitySequence\"')"),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Username = table.Column<string>(type: "VarChar", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "VarChar", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "VarChar", maxLength: 500, nullable: true),
                    FirstName = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "VarChar", maxLength: 100, nullable: true),
                    RegistrationType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoogleTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DomainEntitySequence\"')"),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessToken = table.Column<string>(type: "VarChar", maxLength: 5000, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Scope = table.Column<string>(type: "VarChar", maxLength: 1000, nullable: false),
                    TokenType = table.Column<string>(type: "VarChar", maxLength: 100, nullable: false),
                    RefreshToken = table.Column<string>(type: "VarChar", maxLength: 1000, nullable: false),
                    IdToken = table.Column<string>(type: "VarChar", maxLength: 5000, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DomainEntitySequence\"')"),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "VarChar", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "VarChar", maxLength: 2000, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotionTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DomainEntitySequence\"')"),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    UserId = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleTokens_Uuid",
                table: "GoogleTokens",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId",
                table: "Notes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Uuid",
                table: "Notes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotionTokens_UserId",
                table: "NotionTokens",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotionTokens_Uuid",
                table: "NotionTokens",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Uuid",
                table: "Users",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleTokens");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "NotionTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropSequence(
                name: "DomainEntitySequence");
        }
    }
}
