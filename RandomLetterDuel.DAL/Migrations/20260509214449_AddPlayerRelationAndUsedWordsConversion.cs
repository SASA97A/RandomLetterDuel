using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomLetterDuel.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerRelationAndUsedWordsConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameRooms_GameRoomEntityId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GameRoomEntityId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GameRoomEntityId",
                table: "Players");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameRoomId",
                table: "Players",
                column: "GameRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GameRoomId",
                table: "Players");

            migrationBuilder.AddColumn<Guid>(
                name: "GameRoomEntityId",
                table: "Players",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameRoomEntityId",
                table: "Players",
                column: "GameRoomEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameRooms_GameRoomEntityId",
                table: "Players",
                column: "GameRoomEntityId",
                principalTable: "GameRooms",
                principalColumn: "Id");
        }
    }
}
