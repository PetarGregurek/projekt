using BoardGameReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<GameType> GameTypes { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Review -> Game (many reviews per game, restrict delete to avoid cascade conflict)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> User (many reviews per user)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event -> Game (many events per game)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Game)
                .WithMany(g => g.Events)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Game -> Publisher
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Publisher)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Game -> Category
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Games)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Game -> GameType
            modelBuilder.Entity<Game>()
                .HasOne(g => g.GameType)
                .WithMany(t => t.Games)
                .HasForeignKey(g => g.GameTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed data ────────────────────────────────────────────────────

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Strategy",  Description = "Games focused on planning and tactics", AgeGroup = "12+", Difficulty = Difficulty.Hard,   Popularity = 95 },
                new Category { Id = 2, Name = "Adventure", Description = "Games with exploration and narrative",  AgeGroup = "14+", Difficulty = Difficulty.Medium, Popularity = 85 },
                new Category { Id = 3, Name = "Fantasy",   Description = "Games set in magical worlds",          AgeGroup = "13+", Difficulty = Difficulty.Hard,   Popularity = 90 }
            );

            modelBuilder.Entity<GameType>().HasData(
                new GameType { Id = 1, Name = "Board Game", Description = "Traditional board game" },
                new GameType { Id = 2, Name = "Card Game",  Description = "Game played with cards" },
                new GameType { Id = 3, Name = "Dice Game",  Description = "Game using dice" }
            );

            modelBuilder.Entity<Publisher>().HasData(
                new Publisher { Id = 1, Name = "Catan Studio",   Country = "Germany" },
                new Publisher { Id = 2, Name = "Asmodee",        Country = "France"  },
                new Publisher { Id = 3, Name = "Games Workshop", Country = "UK"      }
            );

            modelBuilder.Entity<Game>().HasData(
                new Game { Id = 1, Name = "Catan",                Description = "Trade, build, and settle the island of Catan.", YearPublished = 1995, MinPlayers = 2, MaxPlayers = 4, Difficulty = Difficulty.Medium, GameTypeId = 1, PublisherId = 1, CategoryId = 1 },
                new Game { Id = 2, Name = "Magic: The Gathering", Description = "Collectible card game of strategy.",            YearPublished = 1993, MinPlayers = 2, MaxPlayers = 2, Difficulty = Difficulty.Hard,   GameTypeId = 2, PublisherId = 2, CategoryId = 1 },
                new Game { Id = 3, Name = "Warhammer 40K",        Description = "Miniatures game in a sci-fi universe.",         YearPublished = 1987, MinPlayers = 2, MaxPlayers = 4, Difficulty = Difficulty.Hard,   GameTypeId = 1, PublisherId = 3, CategoryId = 3 }
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "BoardGameMaster", Email = "master@boardgames.com",   HashedPassword = "hashed_password_1", Country = "Croatia", Age = 28 },
                new User { Id = 2, Username = "StrategyKing",    Email = "strategy@boardgames.com", HashedPassword = "hashed_password_2", Country = "Serbia",  Age = 35 },
                new User { Id = 3, Username = "CasualGamer",     Email = "casual@boardgames.com",   HashedPassword = "hashed_password_3", Country = "Bosnia",  Age = 23 }
            );

            modelBuilder.Entity<Review>().HasData(
                new Review { Id = 1, Title = "Excellent Classic Game",   Rating = 5, Comment = "Catan is fantastic for game nights.",              IsRecommended = true,  CreatedAt = new DateTime(2026, 4, 5),  GameId = 1, UserId = 1 },
                new Review { Id = 2, Title = "Complex but Rewarding",    Rating = 4, Comment = "Magic: The Gathering is deep and strategic.",      IsRecommended = true,  CreatedAt = new DateTime(2026, 4, 15), GameId = 2, UserId = 2 },
                new Review { Id = 3, Title = "Not for Everyone",         Rating = 3, Comment = "Warhammer 40K is impressive but expensive.",       IsRecommended = false, CreatedAt = new DateTime(2026, 4, 25), GameId = 3, UserId = 3 }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { Id = 1, Name = "Catan Tournament",      GameId = 1, StartDateTime = new DateTime(2026, 6, 5,  18, 0, 0), EndDateTime = new DateTime(2026, 6, 5,  22, 0, 0), Location = "Board Game Cafe - Zagreb"    },
                new Event { Id = 2, Name = "Magic Draft Night",     GameId = 2, StartDateTime = new DateTime(2026, 6, 12, 19, 0, 0), EndDateTime = new DateTime(2026, 6, 12, 23, 0, 0), Location = "Card Shop Downtown"          },
                new Event { Id = 3, Name = "Warhammer 40K Campaign",GameId = 3, StartDateTime = new DateTime(2026, 6, 20, 17, 0, 0), EndDateTime = new DateTime(2026, 6, 20, 21, 0, 0), Location = "Gaming Studio - Novi Sad"    }
            );
        }
    }
}
