using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomLetterDuel.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerWin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WinnerId",
                table: "GameRooms",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "GameRooms");
        }
    }
}
