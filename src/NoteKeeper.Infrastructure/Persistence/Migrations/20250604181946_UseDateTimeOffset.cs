using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteKeeper.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UseDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RegistrationType",
                table: "Users",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<byte>(
                name: "Origin",
                table: "Notes",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Notes");

            migrationBuilder.AlterColumn<int>(
                name: "RegistrationType",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint");
        }
    }
}
