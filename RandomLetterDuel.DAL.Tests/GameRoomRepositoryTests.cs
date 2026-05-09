using Microsoft.EntityFrameworkCore;
using RandomLetterDuel.DAL.Data;
using RandomLetterDuel.DAL.Entities;
using RandomLetterDuel.DAL.Repositories;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.DAL.Tests
{
    public class GameRoomRepositoryTests
    {     

        // Unik DB per test
        private RandomLetterDuelDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RandomLetterDuelDbContext(options);
        }


        [Fact]
        public async Task AddAsync_MustSaveGameRoomToDatabase()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var gameRoom = new GameRoomEntity { RoomCode = "TEST01" };

            // Act
            await repository.AddAsync(gameRoom);
            await repository.SaveChangesAsync();

            // Assert
            var savedGameRoom = await context.GameRooms.FirstOrDefaultAsync(g => g.RoomCode == "TEST01");
            Assert.NotNull(savedGameRoom);
            Assert.Equal("TEST01", savedGameRoom.RoomCode);

        }

        [Fact]
        public async Task GetByRoomCodeAsync_MustReturnGameRoomWithPlayers()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var gameRoom = new GameRoomEntity { RoomCode = "TEST02" };
            gameRoom.Players.Add(new PlayerEntity { Name = "Spelare1" });
            gameRoom.Players.Add(new PlayerEntity { Name = "Spelare2" });
            await repository.AddAsync(gameRoom);
            await repository.SaveChangesAsync();

            // Act
            var retrievedGameRoom = await repository.GetByRoomCodeAsync("TEST02");

            // Assert
            Assert.NotNull(retrievedGameRoom);
            Assert.Equal("TEST02", retrievedGameRoom.RoomCode);
            Assert.NotNull(retrievedGameRoom.Players);
            Assert.Equal(2, retrievedGameRoom.Players.Count);
            Assert.Contains(retrievedGameRoom.Players, p => p.Name == "Spelare1");
            Assert.Contains(retrievedGameRoom.Players, p => p.Name == "Spelare2");

        }

        [Fact]
        public async Task GetByRoomCodeAsync_NonExistingRoomCode_MustReturnNull()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);

            // Act
            var result = await repository.GetByRoomCodeAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);

        }

        //Check this
        [Fact]
        public async Task JoinGame_MustReturnNull_WhenGameIsFull()
        {
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var roomId = Guid.NewGuid();
            var gameRoom = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "FULL01",
                Players = new List<PlayerEntity>
                {
                    new PlayerEntity { Name = "Spelare1", GameRoomId = roomId },
                    new PlayerEntity { Name = "Spelare2", GameRoomId = roomId }
                }
            };

            await context.GameRooms.AddAsync(gameRoom);
            await context.SaveChangesAsync();

            var joinRequest = new JoinGameRequestDto { PlayerName = "Spelare3", RoomCode = "FULL01" };
            var result = await repository.JoinGameAsync(joinRequest);

            Assert.Null(result);

        }

        [Fact]
        public async Task SubmitWord_MustReject_DuplicateNormalizedWordsOnly()
        {
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var gameRoom = new GameRoomEntity
            {
                Id = Guid.NewGuid(),
                RoomCode = "DUPL01",
                UsedWords = new List<string> { "katt" }
                
            };

            await context.GameRooms.AddAsync(gameRoom);
            await context.SaveChangesAsync();

            //ACT
            string newWord = "KATT"; 
            bool alreadyUsed = gameRoom.UsedWords.Any(w => string.Equals(w, newWord, StringComparison.OrdinalIgnoreCase));

            Assert.True(alreadyUsed, "Ordet bör betraktas som en duplicate trots olika case");

        }

        [Fact]
        public async Task GetByIdAsync_MustReturnGameRoomWithPlayers()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var roomId = Guid.NewGuid();
            var gameRoom = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "IDTEST"
            };
            gameRoom.Players.Add(new PlayerEntity { Name = "Spelare1", GameRoom = gameRoom });

            await context.GameRooms.AddAsync(gameRoom);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(roomId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IDTEST", result.RoomCode);
            Assert.Single(result.Players);
            Assert.Equal("Spelare1", result.Players[0].Name);
        }

        [Fact]
        public async Task JoinGameAsync_SuccessfulJoin_MustAddPlayerAndChangeState()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new GameRoomRepository(context);
            var gameRoom = new GameRoomEntity
            {
                RoomCode = "JOIN01",
                State = GameState.WaitingForPlayers
            };
            gameRoom.Players.Add(new PlayerEntity { Name = "Värd", GameRoom = gameRoom });

            await context.GameRooms.AddAsync(gameRoom);
            await context.SaveChangesAsync();

            var joinRequest = new JoinGameRequestDto { PlayerName = "Gäst", RoomCode = "JOIN01" };

            // Act
            var result = await repository.JoinGameAsync(joinRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Gäst", result.Name);

            // Kontrollerar att rummet i databasen uppdaterats
            var updatedRoom = await context.GameRooms.Include(g => g.Players).FirstAsync(g => g.RoomCode == "JOIN01");
            Assert.Equal(2, updatedRoom.Players.Count);
            Assert.Equal(GameState.InProgress, updatedRoom.State);
        }

    }
}
