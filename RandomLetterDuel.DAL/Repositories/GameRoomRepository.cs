using Microsoft.EntityFrameworkCore;
using RandomLetterDuel.DAL.Data;
using RandomLetterDuel.DAL.Entities;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.DAL.Repositories
{
    public class GameRoomRepository : IGameRoomRepository
    {

        private readonly RandomLetterDuelDbContext _context;

        public GameRoomRepository(RandomLetterDuelDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(GameRoomEntity gameRoom)
        {
            await _context.GameRooms.AddAsync(gameRoom);
        }

        public async Task<GameRoomEntity?> GetByRoomCodeAsync(string roomCode)
        {
            return await _context.GameRooms
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.RoomCode == roomCode);
        }

        public async Task<GameRoomEntity?> GetByIdAsync(Guid id)
        {
            return await _context.GameRooms
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<PlayerEntity?> JoinGameAsync(JoinGameRequestDto request)
        {
            var gameRoom = await _context.GameRooms
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == request.RoomCode);

            if (gameRoom == null || gameRoom.Players.Count >= 2)
            {
                return null;
            }

            var newPlayer = new PlayerEntity
            {
                Id = Guid.NewGuid(),
                Name = request.PlayerName.Trim(),
                Score = 0,
                GameRoomId = gameRoom.Id,
                GameRoom = gameRoom
            };

            //Fix: Sätter tur till den första spelaren som går med i rummet
            gameRoom.CurrentTurnPlayerId = gameRoom.Players.First().Id;

            await _context.Players.AddAsync(newPlayer);

            //gameRoom.Players.Add(newPlayer);
            gameRoom.State = GameState.InProgress;

            await _context.SaveChangesAsync();

            // Reload players
            await _context.Entry(gameRoom)
                .Collection(g => g.Players)
                .LoadAsync();

            return newPlayer;
        }
    }
}
