using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UseNpgsqlTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "Varchar",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "Varchar",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "Varchar",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceName",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceId",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceIconUrl",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NotionId",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotionEmail",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BotId",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "NotionTokens",
                type: "Varchar",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 5000);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notes",
                type: "Varchar",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notes",
                type: "Varchar",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "GoogleTokens",
                type: "Varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "GoogleTokens",
                type: "Varchar",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleTokens",
                type: "Varchar",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "IdToken",
                table: "GoogleTokens",
                type: "Varchar",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "GoogleTokens",
                type: "Varchar",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VarChar",
                oldMaxLength: 5000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "VarChar",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VarChar",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "VarChar",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceName",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceId",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceIconUrl",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NotionId",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotionEmail",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BotId",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "NotionTokens",
                type: "VarChar",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 5000);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notes",
                type: "VarChar",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notes",
                type: "VarChar",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "IdToken",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "GoogleTokens",
                type: "VarChar",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Varchar",
                oldMaxLength: 5000);
        }
    }
}
