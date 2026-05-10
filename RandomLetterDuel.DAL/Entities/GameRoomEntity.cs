using RandomLetterDuel.DTO;
using System.ComponentModel.DataAnnotations;

namespace RandomLetterDuel.DAL.Entities
{
    public class GameRoomEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(6)]
        public string RoomCode { get; set; } = string.Empty;
        public GameState State { get; set; } = GameState.WaitingForPlayers;
        public char? RequiredLetter { get; set; }
        public Guid? CurrentTurnPlayerId { get; set; }
        public Guid? WinnerId { get; set; }
        public List<PlayerEntity> Players { get; set; } = new();
        public List<string> UsedWords { get; set; } = new();
    }
}
