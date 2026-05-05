using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BoardGameReviews.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "AgeGroup", "Description", "Difficulty", "Name", "Popularity" },
                values: new object[,]
                {
                    { 1, "12+", "Games focused on planning and tactics", 2, "Strategy", 95 },
                    { 2, "14+", "Games with exploration and narrative", 1, "Adventure", 85 },
                    { 3, "13+", "Games set in magical worlds", 2, "Fantasy", 90 }
                });

            migrationBuilder.InsertData(
                table: "GameTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Traditional board game", "Board Game" },
                    { 2, "Game played with cards", "Card Game" },
                    { 3, "Game using dice", "Dice Game" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "Country", "Name" },
                values: new object[,]
                {
                    { 1, "Germany", "Catan Studio" },
                    { 2, "France", "Asmodee" },
                    { 3, "UK", "Games Workshop" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Age", "Country", "Email", "HashedPassword", "Username" },
                values: new object[,]
                {
                    { 1, 28, "Croatia", "master@boardgames.com", "hashed_password_1", "BoardGameMaster" },
                    { 2, 35, "Serbia", "strategy@boardgames.com", "hashed_password_2", "StrategyKing" },
                    { 3, 23, "Bosnia", "casual@boardgames.com", "hashed_password_3", "CasualGamer" }
                });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "CategoryId", "Description", "Difficulty", "GameTypeId", "MaxPlayers", "MinPlayers", "Name", "PublisherId", "YearPublished" },
                values: new object[,]
                {
                    { 1, 1, "Trade, build, and settle the island of Catan.", 1, 1, 4, 2, "Catan", 1, 1995 },
                    { 2, 1, "Collectible card game of strategy.", 2, 2, 2, 2, "Magic: The Gathering", 2, 1993 },
                    { 3, 3, "Miniatures game in a sci-fi universe.", 2, 1, 4, 2, "Warhammer 40K", 3, 1987 }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "EndDateTime", "GameId", "Location", "Name", "StartDateTime" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 5, 22, 0, 0, 0, DateTimeKind.Unspecified), 1, "Board Game Cafe - Zagreb", "Catan Tournament", new DateTime(2026, 6, 5, 18, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 6, 12, 23, 0, 0, 0, DateTimeKind.Unspecified), 2, "Card Shop Downtown", "Magic Draft Night", new DateTime(2026, 6, 12, 19, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 6, 20, 21, 0, 0, 0, DateTimeKind.Unspecified), 3, "Gaming Studio - Novi Sad", "Warhammer 40K Campaign", new DateTime(2026, 6, 20, 17, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "GameId", "IsRecommended", "Rating", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, "Catan is fantastic for game nights.", new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, 5, "Excellent Classic Game", 1 },
                    { 2, "Magic: The Gathering is deep and strategic.", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, true, 4, "Complex but Rewarding", 2 },
                    { 3, "Warhammer 40K is impressive but expensive.", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, 3, "Not for Everyone", 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GameTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GameTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GameTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
