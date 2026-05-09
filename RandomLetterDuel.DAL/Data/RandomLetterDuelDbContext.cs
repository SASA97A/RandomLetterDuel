using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RandomLetterDuel.DAL.Entities;

namespace RandomLetterDuel.DAL.Data
{
    public class RandomLetterDuelDbContext : DbContext
    {
        public RandomLetterDuelDbContext(DbContextOptions<RandomLetterDuelDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.GameRoomEntity> GameRooms { get; set; }
        public DbSet<Entities.PlayerEntity> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameRoomEntity>()
                .HasMany(g => g.Players)
                .WithOne(p => p.GameRoom)
                .HasForeignKey(p => p.GameRoomId);

            modelBuilder.Entity<GameRoomEntity>()
                .Property(e => e.UsedWords)
                .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2), // Hur man jämför två listor
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Hur man skapar ett hash-värde
                c => c.ToList() // Hur man skapar en kopia
        ));

            base.OnModelCreating(modelBuilder);
        }
    }
}
