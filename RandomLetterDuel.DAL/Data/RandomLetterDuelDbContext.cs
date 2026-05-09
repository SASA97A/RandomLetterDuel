using Microsoft.EntityFrameworkCore;

namespace RandomLetterDuel.DAL.Data
{
    public class RandomLetterDuelDbContext : DbContext
    {
        public RandomLetterDuelDbContext(DbContextOptions<RandomLetterDuelDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.GameRoomEntity> GameRooms { get; set; }
        public DbSet<Entities.PlayerEntity> Players { get; set; }
    }
}
