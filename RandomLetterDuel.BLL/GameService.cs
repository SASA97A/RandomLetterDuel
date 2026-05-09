using RandomLetterDuel.DTO;

namespace RandomLetterDuel.BLL
{
    public class GameService
    {
        private readonly Random _random = new Random();

        public GameRoomResponseDto CreateGame(CreateGameRequestDto request)
        {
            // Validerar input
            if (request == null ) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                throw new ArgumentException("Spelarenamn är ogiltigt");
            }

            string roomCode = GenerateRoomCode(6);

            return new GameRoomResponseDto
            {
                Id = Guid.NewGuid(),
                RoomCode = roomCode,
                State = GameState.WaitingForPlayers,
                Players = new List<PlayerDto>
                {
                    new PlayerDto 
                    { 
                        Id = Guid.NewGuid(),
                        Name = request.PlayerName.Trim(),
                        Score = 0 }
                    }
                };
        }


        public GameRoomResponseDto JoinGame(JoinGameRequestDto request)
        {
            // 1. Här skulle du normalt hämta spelet från DAL via RoomCode
            // 2. Kontrollera att namnet är giltigt
            // 3. Lägg till spelaren i listan
            // 4. Ändra State till InProgress

            // (För prototypen kan du simulera detta genom att returnera ett uppdaterat objekt)
            return new GameRoomResponseDto
            {
                State = GameState.InProgress,
                // ... övrig data
            };
        }


        private string GenerateRoomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }






    }
}
