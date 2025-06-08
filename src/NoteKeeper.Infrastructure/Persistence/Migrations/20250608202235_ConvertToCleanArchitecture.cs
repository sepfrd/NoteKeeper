using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToCleanArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoogleTokens_Users_UserId",
                table: "GoogleTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_NotionTokens_Users_UserId",
                table: "NotionTokens");

            migrationBuilder.DropIndex(
                name: "IX_NotionTokens_UserId",
                table: "NotionTokens");

            migrationBuilder.DropIndex(
                name: "IX_NotionTokens_Uuid",
                table: "NotionTokens");

            migrationBuilder.DropIndex(
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens");

            migrationBuilder.DropIndex(
                name: "IX_GoogleTokens_Uuid",
                table: "GoogleTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "NotionTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "NotionTokens");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "NotionTokens");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "GoogleTokens");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "NotionTokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "nextval('\"DomainEntitySequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GoogleTokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "nextval('\"DomainEntitySequence\"')")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "ExternalProviderAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"DomainEntitySequence\"')"),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    ProviderType = table.Column<byte>(type: "smallint", nullable: false),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalProviderAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalProviderAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalProviderAccounts_UserId",
                table: "ExternalProviderAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalProviderAccounts_Uuid",
                table: "ExternalProviderAccounts",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalProviderAccounts");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "NotionTokens",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('\"DomainEntitySequence\"')",
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "NotionTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "NotionTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "NotionTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GoogleTokens",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('\"DomainEntitySequence\"')",
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "GoogleTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleTokens_Uuid",
                table: "GoogleTokens",
                column: "Uuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoogleTokens_Users_UserId",
                table: "GoogleTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotionTokens_Users_UserId",
                table: "NotionTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
