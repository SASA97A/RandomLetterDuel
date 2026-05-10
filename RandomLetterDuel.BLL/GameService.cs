using RandomLetterDuel.DAL.Entities;
using RandomLetterDuel.DAL.Repositories;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.BLL
{
    public class GameService
    {
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly Random _random = new Random();

        public GameService(IGameRoomRepository gameRoomRepository) 
        {
            _gameRoomRepository = gameRoomRepository;
        }

        public async Task<GameRoomResponseDto> CreateGame(CreateGameRequestDto request)
        {
            // Validerar input
            if (request == null ) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.PlayerName))
            {
                throw new ArgumentException("Spelarenamn är ogiltigt");
            }

            var entity = new GameRoomEntity
            {
                Id = Guid.NewGuid(),
                RoomCode = GenerateRoomCode(6),
                State = GameState.WaitingForPlayers,
                UsedWords = new List<string>()
            };

            entity.Players.Add(new PlayerEntity
            {
                Id = Guid.NewGuid(),
                Name = request.PlayerName.Trim(),
                Score = 0
            });

            await _gameRoomRepository.AddAsync(entity);
            await _gameRoomRepository.SaveChangesAsync();

            return MapToResponse(entity);

        }


        public async Task<GameRoomResponseDto> JoinGame(JoinGameRequestDto request)
        {
            var playerEntity = await _gameRoomRepository.JoinGameAsync(request);

            if (playerEntity == null)
            {
                var existingRoom = await _gameRoomRepository.GetByRoomCodeAsync(request.RoomCode);

                if (existingRoom == null) return null;

                return MapToResponse(existingRoom);
            }

            var updatedRoom = await _gameRoomRepository.GetByRoomCodeAsync(request.RoomCode);
            return MapToResponse(updatedRoom!);
        }


        public async Task<GameRoomResponseDto> SubmitWordAsync(MakeMoveRequestDto request)
        {
            
            var gameRoom = await _gameRoomRepository.GetByIdAsync(request.GameId);

            if (gameRoom == null) { throw new Exception("Spelet hittades inte"); }

            if (gameRoom.State != GameState.InProgress)
            {
                throw new InvalidOperationException("Spelet är inte igång (väntar på spelare eller är avslutat).");
            }

            if (gameRoom.CurrentTurnPlayerId != request.PlayerId)
            {
                throw new InvalidOperationException("Det är inte din tur!");
            }

            string word = request.Word?.Trim().ToLower();

            //Regler för ord

            if (string.IsNullOrWhiteSpace(request.Word))
            {
                throw new ArgumentException("Ordet får inte vara tomt.");
            }

            if (word.Length < 3)
            {
                throw new ArgumentException("Ordet är för kort.");
            }

            if (gameRoom.RequiredLetter.HasValue && !word.StartsWith(gameRoom.RequiredLetter.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Ordet måste börja med bokstaven '{gameRoom.RequiredLetter.Value}'");
            }

            if (gameRoom.UsedWords.Contains(word))
            {
                throw new ArgumentException("Ordet har redan använts");
            }

            if (word.Length > 20)
            {
                throw new ArgumentException("Ordet är för långt (max 20 tecken).");
            }


            //Väljer nästa bokstav
            char nextLetter = char.ToUpper(word.Last());
            gameRoom.RequiredLetter = nextLetter;

            //Lägger till ordet i listan över använda ord
            gameRoom.UsedWords.Add(word);

            //uppdatera poäng
            var player = gameRoom.Players.FirstOrDefault(p => p.Id == request.PlayerId);
            if (player != null) 
            {
                player.Score += word.Length;

                if (player.Score >= 50)
                {
                    gameRoom.State = GameState.GameFinished;
                    gameRoom.WinnerId = player.Id;

                    await _gameRoomRepository.SaveChangesAsync();
                    return MapToResponse(gameRoom);
                }
            }
            
            //Växla tur
            var nextPlayer = gameRoom.Players.FirstOrDefault(p => p.Id != request.PlayerId);
            if (nextPlayer != null) gameRoom.CurrentTurnPlayerId = nextPlayer.Id;

            //Spara till DB
            await _gameRoomRepository.SaveChangesAsync();

            //Returnera uppdaterad spelstatus till UI
            return MapToResponse(gameRoom);
            
        }      

        private string GenerateRoomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private GameRoomResponseDto MapToResponse(GameRoomEntity entity)
        {
            return new GameRoomResponseDto
            {
                Id = entity.Id,
                RoomCode = entity.RoomCode,
                State = entity.State,
                RequiredLetter = entity.RequiredLetter,
                CurrentTurnPlayerId = entity.CurrentTurnPlayerId ?? Guid.Empty,
                UsedWords = entity.UsedWords,
                WinnerId = entity.WinnerId,
                Players = entity.Players.Select(p => new PlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Score = p.Score
                }).ToList(),
            };
        }


    }
}
