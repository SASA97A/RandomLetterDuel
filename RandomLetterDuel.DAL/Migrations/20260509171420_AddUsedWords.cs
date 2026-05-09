using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomLetterDuel.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsedWords",
                table: "GameRooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedWords",
                table: "GameRooms");
        }
    }
}
