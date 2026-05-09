using System.ComponentModel.DataAnnotations;

namespace RandomLetterDuel.DAL.Entities
{
    public class PlayerEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }

        public Guid GameRoomId { get; set; }

        public GameRoomEntity GameRoom { get; set; } = null!;
    }
}
