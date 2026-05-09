using RandomLetterDuel.DTO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RandomLetterDuel.BLL.Tests
{
    public class GameServiceTests
    {
        // Gör ett test på att få WaitingForPlayers vid spelrum skapelse
        [Fact]
        public void CreateGame_MustReturnNewGameInWaitingState()
        {
            var service = new GameService();

            var result = service.CreateGame("Spelare 1");

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
        public  void CreateGame_InvalidName_ShouldThrowArgumentException(string invalidName)
        {
            var service = new GameService();
            var result = new CreateGameRequestDto { PlayerName = invalidName };

             Assert.Throws<ArgumentException>(() => service.CreateGame(result));
        }

        [Fact]
        public void CreateGame_MustGenerateNonEmptyRoomCode()
        {
            var service = new GameService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = service.CreateGame(request);

            Assert.False(string.IsNullOrEmpty(result.RoomCode));
        }

        [Fact]
        public void CreateGame_RoomCode_MustBeExactlySixCharacters()
        {
            var service = new GameService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = service.CreateGame(request);

            Assert.Equal(6, result.RoomCode.Length);
        }

        [Theory]
        [InlineData("^[A-Z0-9]+$")]
        public void CreateGame_RoomCode_ShouldFollowAlphanumericFormat(string pattern)
        {
            var service = new GameService();
            var request = new CreateGameRequestDto { PlayerName = "TestSpelare" };

            var result = service.CreateGame(request);

            Assert.Matches(pattern, result.RoomCode);
        }

        [Fact]
        public void CreateGame_TwoGames_MustHaveDifferentRoomCodes()
        {
            var service = new GameService();
            var request = new CreateGameRequestDto { PlayerName = "Spelare" };

            var game1 = service.CreateGame(request);
            var game2 = service.CreateGame(request);

            Assert.NotEqual(game1, game2);
        }

        [Fact]
        public void JoinGame_ValidCode_MustChangeStateToInProgress()
        {
            var service = new GameService();
            var createRequest = new CreateGameRequestDto { PlayerName = "Spelare 1" };
            var game = service.CreateGame(createRequest);

            var joinRequest = new JoinGameRequestDto
            {
                PlayerName = "Spelare 2",
                RoomCode = game.RoomCode
            };


            var updatedGame = service.JoinGame(joinRequest);

            Assert.Equal(GameState.InProgress, updatedGame.State);
            Assert.Equal(2, updatedGame.Players.Count);

        }






    }   
}
