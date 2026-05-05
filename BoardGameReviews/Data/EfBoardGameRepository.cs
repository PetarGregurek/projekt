using BoardGameReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Data
{
    public class EfBoardGameRepository : IBoardGameRepository
    {
        private readonly AppDbContext _db;

        public EfBoardGameRepository(AppDbContext db)
        {
            _db = db;
        }

        // ── Collections (index) ─────────────────────────────────────────────

        public Task<List<Game>> GetAllGamesAsync() =>
            _db.Games
               .Include(g => g.GameType)
               .Include(g => g.Publisher)
               .Include(g => g.Category)
               .Include(g => g.Reviews)
               .Include(g => g.Events)
               .OrderBy(g => g.Name)
               .ToListAsync();

        public Task<List<Category>> GetAllCategoriesAsync() =>
            _db.Categories.OrderBy(c => c.Name).ToListAsync();

        public Task<List<GameType>> GetAllGameTypesAsync() =>
            _db.GameTypes.OrderBy(t => t.Name).ToListAsync();

        public Task<List<Publisher>> GetAllPublishersAsync() =>
            _db.Publishers.OrderBy(p => p.Name).ToListAsync();

        public Task<List<User>> GetAllUsersAsync() =>
            _db.Users.OrderBy(u => u.Username).ToListAsync();

        public Task<List<Review>> GetAllReviewsAsync() =>
            _db.Reviews
               .Include(r => r.Game)
               .Include(r => r.User)
               .OrderByDescending(r => r.CreatedAt)
               .ToListAsync();

        public Task<List<Event>> GetAllEventsAsync() =>
            _db.Events
               .Include(e => e.Game)
               .OrderBy(e => e.StartDateTime)
               .ToListAsync();

        // ── Detail lookups ───────────────────────────────────────────────────

        public Task<Game?> GetGameWithDetailsAsync(int id) =>
            _db.Games
               .Include(g => g.GameType)
               .Include(g => g.Publisher)
               .Include(g => g.Category)
               .Include(g => g.Reviews).ThenInclude(r => r.User)
               .Include(g => g.Events)
               .FirstOrDefaultAsync(g => g.Id == id);

        public Task<Category?> GetCategoryWithGamesAsync(int id) =>
            _db.Categories
               .Include(c => c.Games)
               .FirstOrDefaultAsync(c => c.Id == id);

        public Task<GameType?> GetGameTypeWithGamesAsync(int id) =>
            _db.GameTypes
               .Include(t => t.Games)
               .FirstOrDefaultAsync(t => t.Id == id);

        public Task<Publisher?> GetPublisherWithGamesAsync(int id) =>
            _db.Publishers
               .Include(p => p.Games)
               .FirstOrDefaultAsync(p => p.Id == id);

        public Task<User?> GetUserWithReviewsAsync(int id) =>
            _db.Users
               .Include(u => u.Reviews).ThenInclude(r => r.Game)
               .FirstOrDefaultAsync(u => u.Id == id);

        public Task<Review?> GetReviewWithDetailsAsync(int id) =>
            _db.Reviews
               .Include(r => r.Game)
               .Include(r => r.User)
               .FirstOrDefaultAsync(r => r.Id == id);

        public Task<Event?> GetEventWithGameAsync(int id) =>
            _db.Events
               .Include(e => e.Game)
               .FirstOrDefaultAsync(e => e.Id == id);

        // ── Home page ────────────────────────────────────────────────────────

        public Task<int> CountGamesAsync()   => _db.Games.CountAsync();
        public Task<int> CountReviewsAsync() => _db.Reviews.CountAsync();
        public Task<int> CountUsersAsync()   => _db.Users.CountAsync();
        public Task<int> CountEventsAsync()  => _db.Events.CountAsync();

        public async Task<List<Game>> GetTopRatedGamesAsync(int count)
        {
            var games = await _db.Games
                .Include(g => g.Reviews)
                .Include(g => g.Publisher)
                .ToListAsync();

            return games
                .OrderByDescending(g => g.Reviews.Count > 0 ? g.Reviews.Average(r => r.Rating) : 0)
                .ThenByDescending(g => g.Reviews.Count)
                .Take(count)
                .ToList();
        }

        public Task<List<Event>> GetUpcomingEventsAsync(int count) =>
            _db.Events
               .Include(e => e.Game)
               .Where(e => e.EndDateTime >= DateTime.Now)
               .OrderBy(e => e.StartDateTime)
               .Take(count)
               .ToListAsync();

        public async Task<List<Category>> GetPopularCategoriesAsync(int count)
        {
            var categories = await _db.Categories
                .Include(c => c.Games).ThenInclude(g => g.Reviews)
                .ToListAsync();

            return categories
                .OrderByDescending(c => c.Games.Sum(g => g.Reviews.Count))
                .Take(count)
                .ToList();
        }
    }
}
