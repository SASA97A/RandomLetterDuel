using Azure.Core;
using Microsoft.EntityFrameworkCore;
using RandomLetterDuel.DAL.Data;
using RandomLetterDuel.DAL.Entities;
using RandomLetterDuel.DAL.Repositories;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.BLL.Tests
{
    public class GameServiceTests
    {
        private GameService GetService()
        {
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            return new GameService(repo);
        }


        // Gör ett test på att få WaitingForPlayers vid spelrum skapelse
        [Fact]
        public async Task CreateGame_MustReturnNewGameInWaitingState()
        {
            var service = GetService();
            var request = new CreateGameRequestDto { PlayerName = "Spelare 1" };

            var result = await service.CreateGame(request);

            Assert.NotNull(result);
            Assert.Equal(GameState.WaitingForPlayers, result.State);
            Assert.Single(result.Players);
            Assert.Equal("Spelare 1", result.Players[0].Name);
        }

        //Testar så att det inte går att ha ett tomt namn/bara mellanslag.
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateGame_InvalidName_ShouldThrowArgumentException(string invalidName)
        {
            var service = GetService();
            var result = new CreateGameRequestDto { PlayerName = invalidName };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateGame(result));
        }

        [Fact]
        public async Task CreateGame_MustGenerateNonEmptyRoomCode()
        {
            var service = GetService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = await service.CreateGame(request);

            Assert.False(string.IsNullOrEmpty(result.RoomCode));
        }

        [Fact]
        public async Task CreateGame_RoomCode_MustBeExactlySixCharacters()
        {
            var service = GetService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = await service.CreateGame(request);

            Assert.Equal(6, result.RoomCode.Length);
        }

        [Theory]
        [InlineData("^[A-Z0-9]+$")]
        public async Task CreateGame_RoomCode_ShouldFollowAlphanumericFormat(string pattern)
        {
            var service = GetService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = await service.CreateGame(request);

            Assert.Matches(pattern, result.RoomCode);
        }

        [Fact]
        public async Task CreateGame_TwoGames_MustHaveDifferentRoomCodes()
        {
            var service = GetService();
            var request = new CreateGameRequestDto { PlayerName = "Spelare" };

            var game1 = await service.CreateGame(request);
            var game2 = await service.CreateGame(request);

            Assert.NotEqual(game1.RoomCode, game2.RoomCode);
        }

        [Fact]
        public async Task JoinGame_ValidCode_MustChangeStateToInProgress()
        {
            var service = GetService();
            var createRequest = new CreateGameRequestDto { PlayerName = "Spelare 1" };
            var game = await service.CreateGame(createRequest);

            var joinRequest = new JoinGameRequestDto
            {
                PlayerName = "Spelare 2",
                RoomCode = game.RoomCode
            };


            var updatedGame = await service.JoinGame(joinRequest);

            Assert.Equal(GameState.InProgress, updatedGame.State);
            Assert.Equal(2, updatedGame.Players.Count);

        }

        [Fact]
        public async Task JoinGame_InvalidCode_MustNotChangeState()
        {
            var service = GetService();
            var createRequest = new CreateGameRequestDto { PlayerName = "Spelare 1" };
            var game = await service.CreateGame(createRequest);

            var joinRequest = new JoinGameRequestDto
            {
                PlayerName = "Spelare 2",
                RoomCode = "FEL-KOD"
            };

            var result = await service.JoinGame(joinRequest);

            Assert.Null(result);
        }


        [Fact]
        public async Task SubmitWord_ValidWord_MustUpdateRequiredLetterToLastChar()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "ABCDEF",
                State = GameState.InProgress,
                RequiredLetter = 'K',
                CurrentTurnPlayerId = playerId
            };
            room.Players.Add(new PlayerEntity { Id = playerId, Name = "P1", GameRoomId = roomId });
            room.Players.Add(new PlayerEntity { Id = Guid.NewGuid(), Name = "P2", GameRoomId = roomId });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerId,
                Word = "KATT"
            };

            // Act
            var result = await service.SubmitWordAsync(request);

            // Assert
            // Ordet slutar på 'T', så nästa bokstav måste vara 'T'
            Assert.Equal('T', result.RequiredLetter);
            Assert.Contains("katt", result.UsedWords);

        }

        [Fact]
        public async Task SubmitWord_InvalidWord_MustThrowArgumentException()
        {
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);
            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "ABCDEF",
                State = GameState.InProgress,
                RequiredLetter = 'K',
                CurrentTurnPlayerId = playerId
            };
            room.Players.Add(new PlayerEntity { Id = playerId, Name = "P1", GameRoomId = roomId });
            room.Players.Add(new PlayerEntity { Id = Guid.NewGuid(), Name = "P2", GameRoomId = roomId });
            context.GameRooms.Add(room);
            await context.SaveChangesAsync();
            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerId,
                Word = "  " // Ogiltigt ord (bara mellanslag)
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitWordAsync(request));

        }

        [Fact]
        public async Task SubmitWord_WrongPlayerTurn_MustThrowInvalidOperationException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerAId = Guid.NewGuid();
            var playerBId = Guid.NewGuid();

            // Skapar ett spel där det är Player A:s tur
            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "TURTST",
                State = GameState.InProgress,
                CurrentTurnPlayerId = playerAId 
            };

            room.Players.Add(new PlayerEntity { Id = playerAId, Name = "Spelare A", GameRoom = room });
            room.Players.Add(new PlayerEntity { Id = playerBId, Name = "Spelare B", GameRoom = room });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            // Spelare B försöker göra ett drag
            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerBId, // FEL SPELARE
                Word = "KATT"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitWordAsync(request));
            Assert.Equal("Det är inte din tur!", exception.Message);
        }

        [Fact]
        public async Task SubmitWord_WordTooLong_MustThrowArgumentException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "LNGTST",
                State = GameState.InProgress,
                CurrentTurnPlayerId = playerId
            };

            room.Players.Add(new PlayerEntity { Id = playerId, Name = "Testare", GameRoom = room });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerId,
                Word = "HETERODISULFIDSTRUKTUR" // 22 tecken
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitWordAsync(request));
            Assert.Contains("för långt", exception.Message);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("KI")]
        [InlineData("Ö")]
        public async Task SubmitWord_WordTooShort_ShouldThrowArgumentException(string shortWord)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "SHRTST",
                State = GameState.InProgress,
                CurrentTurnPlayerId = playerId
            };
            room.Players.Add(new PlayerEntity { Id = playerId, Name = "Testare", GameRoom = room });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerId,
                Word = shortWord
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitWordAsync(request));
            Assert.Equal("Ordet är för kort.", exception.Message);
        }

        [Fact]
        public async Task SubmitWord_ScoreBelowLimit_MustKeepGameInProgress()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "PLAYON",
                State = GameState.InProgress,
                CurrentTurnPlayerId = playerId
            };

            room.Players.Add(new PlayerEntity { Id = playerId, Name = "Spelare 1", Score = 10, GameRoom = room });
            room.Players.Add(new PlayerEntity { Id = Guid.NewGuid(), Name = "Spelare 2", GameRoom = room });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            var request = new MakeMoveRequestDto { GameId = roomId, PlayerId = playerId, Word = "KATT" };

            // Act
            var result = await service.SubmitWordAsync(request);

            // Assert
            Assert.Equal(GameState.InProgress, result.State);
            Assert.Null(result.WinnerId);
            Assert.Equal(14, result.Players.First(p => p.Id == playerId).Score);
        }


        [Theory]
        [InlineData(47, "KATT")] 
        [InlineData(45, "RADIO")] 
        public async Task SubmitWord_ReachingScoreLimit_MustSetGameStateToFinishedAndSetWinner(int initialScore, string winningWord)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RandomLetterDuelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RandomLetterDuelDbContext(options);
            var repo = new GameRoomRepository(context);
            var service = new GameService(repo);

            var roomId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var room = new GameRoomEntity
            {
                Id = roomId,
                RoomCode = "WINNER",
                State = GameState.InProgress,
                CurrentTurnPlayerId = playerId,
                RequiredLetter = winningWord[0]
            };

            // Sätter spelarens poäng nära gränsen
            var player = new PlayerEntity { Id = playerId, Name = "Vinnaren", Score = initialScore, GameRoom = room };
            room.Players.Add(player);
            room.Players.Add(new PlayerEntity { Id = Guid.NewGuid(), Name = "Förloraren", GameRoom = room });

            context.GameRooms.Add(room);
            await context.SaveChangesAsync();

            var request = new MakeMoveRequestDto
            {
                GameId = roomId,
                PlayerId = playerId,
                Word = winningWord
            };

            // Act
            var result = await service.SubmitWordAsync(request);

            // Assert
            Assert.Equal(GameState.GameFinished, result.State);
            Assert.Equal(playerId, result.WinnerId);
            Assert.True(result.Players.First(p => p.Id == playerId).Score >= 50);
        }

    }   
}
