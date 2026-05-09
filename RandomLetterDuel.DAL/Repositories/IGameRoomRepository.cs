using RandomLetterDuel.DAL.Entities;
using RandomLetterDuel.DTO;

namespace RandomLetterDuel.DAL.Repositories
{
    public interface IGameRoomRepository
    {
        Task<GameRoomEntity?> GetByRoomCodeAsync(string roomCode);
        Task AddAsync(GameRoomEntity gameRoom);
        Task SaveChangesAsync();
        Task<PlayerEntity?> JoinGameAsync(JoinGameRequestDto request);
    }
}
