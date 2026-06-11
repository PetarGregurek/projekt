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

        public Task<List<Game>> SearchGamesAsync(string? query, int take = 0)
        {
            var normalizedQuery = query?.Trim();

            var games = _db.Games
                .Include(g => g.GameType)
                .Include(g => g.Publisher)
                .Include(g => g.Category)
                .Include(g => g.Reviews)
                .Include(g => g.Events)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                games = games.Where(g =>
                    g.Name.Contains(normalizedQuery) ||
                    (g.Description != null && g.Description.Contains(normalizedQuery)) ||
                    g.GameType!.Name.Contains(normalizedQuery) ||
                    g.Publisher!.Name.Contains(normalizedQuery) ||
                    g.Category!.Name.Contains(normalizedQuery));
            }

            var ordered = games.OrderBy(g => g.Name);
            return (take > 0 ? ordered.Take(take) : ordered).ToListAsync();
        }

        public Task<List<Category>> GetAllCategoriesAsync() =>
            _db.Categories.OrderBy(c => c.Name).ToListAsync();

        public Task<List<Category>> SearchCategoriesAsync(string? query, int take = 0)
        {
            var normalizedQuery = query?.Trim();

            var categories = _db.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                categories = categories.Where(c =>
                    c.Name.Contains(normalizedQuery) ||
                    (c.Description != null && c.Description.Contains(normalizedQuery)) ||
                    (c.AgeGroup != null && c.AgeGroup.Contains(normalizedQuery)));
            }

            var ordered = categories.OrderBy(c => c.Name);
            return (take > 0 ? ordered.Take(take) : ordered).ToListAsync();
        }

        public Task<List<GameType>> GetAllGameTypesAsync() =>
            _db.GameTypes.OrderBy(t => t.Name).ToListAsync();

        public Task<List<GameType>> SearchGameTypesAsync(string? query, int take = 0)
        {
            var normalizedQuery = query?.Trim();

            var gameTypes = _db.GameTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                gameTypes = gameTypes.Where(t =>
                    t.Name.Contains(normalizedQuery) ||
                    (t.Description != null && t.Description.Contains(normalizedQuery)));
            }

            var ordered = gameTypes.OrderBy(t => t.Name);
            return (take > 0 ? ordered.Take(take) : ordered).ToListAsync();
        }

        public Task<List<Publisher>> GetAllPublishersAsync() =>
            _db.Publishers.OrderBy(p => p.Name).ToListAsync();

        public Task<List<Publisher>> SearchPublishersAsync(string? query, int take = 0)
        {
            var normalizedQuery = query?.Trim();

            var publishers = _db.Publishers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                publishers = publishers.Where(p =>
                    p.Name.Contains(normalizedQuery) ||
                    (p.Country != null && p.Country.Contains(normalizedQuery)));
            }

            var ordered = publishers.OrderBy(p => p.Name);
            return (take > 0 ? ordered.Take(take) : ordered).ToListAsync();
        }

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
               .Include(g => g.Files)
               .FirstOrDefaultAsync(g => g.Id == id);

        public Task<Game?> GetGameForEditAsync(int id) =>
            _db.Games.FirstOrDefaultAsync(g => g.Id == id);

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

        public async Task AddGameAsync(Game game)
        {
            _db.Games.Add(game);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateGameAsync(Game game)
        {
            _db.Games.Update(game);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteGameAsync(Game game)
        {
            game.DeletedAt = DateTime.UtcNow;
            _db.Games.Update(game);
            await _db.SaveChangesAsync();
        }

        public Task<bool> CanDeleteGameAsync(int id) =>
            _db.Games
               .Where(g => g.Id == id)
               .Select(g => !g.Reviews.Any())
               .FirstOrDefaultAsync();

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

        public Task<List<User>> SearchUsersAsync(string? query, int take = 0)
        {
            var normalizedQuery = query?.Trim();
            var users = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                users = users.Where(u =>
                    u.Username.Contains(normalizedQuery) ||
                    u.Email.Contains(normalizedQuery) ||
                    (u.Country != null && u.Country.Contains(normalizedQuery)));
            }
            var ordered = users.OrderBy(u => u.Username);
            return (take > 0 ? ordered.Take(take) : ordered).ToListAsync();
        }

        // ── Category mutations ────────────────────────────────────────────────
        public async Task AddCategoryAsync(Category category) { _db.Categories.Add(category); await _db.SaveChangesAsync(); }
        public async Task UpdateCategoryAsync(Category category) { _db.Categories.Update(category); await _db.SaveChangesAsync(); }
        public async Task DeleteCategoryAsync(Category category) { _db.Categories.Remove(category); await _db.SaveChangesAsync(); }
        public Task<bool> CanDeleteCategoryAsync(int id) =>
            _db.Categories.Where(c => c.Id == id).Select(c => !c.Games.Any()).FirstOrDefaultAsync();

        // ── Event mutations ───────────────────────────────────────────────────
        public async Task AddEventAsync(Event evt) { _db.Events.Add(evt); await _db.SaveChangesAsync(); }
        public async Task UpdateEventAsync(Event evt) { _db.Events.Update(evt); await _db.SaveChangesAsync(); }
        public async Task DeleteEventAsync(Event evt) { _db.Events.Remove(evt); await _db.SaveChangesAsync(); }

        // ── GameType mutations ────────────────────────────────────────────────
        public async Task AddGameTypeAsync(GameType gameType) { _db.GameTypes.Add(gameType); await _db.SaveChangesAsync(); }
        public async Task UpdateGameTypeAsync(GameType gameType) { _db.GameTypes.Update(gameType); await _db.SaveChangesAsync(); }
        public async Task DeleteGameTypeAsync(GameType gameType) { _db.GameTypes.Remove(gameType); await _db.SaveChangesAsync(); }
        public Task<bool> CanDeleteGameTypeAsync(int id) =>
            _db.GameTypes.Where(t => t.Id == id).Select(t => !t.Games.Any()).FirstOrDefaultAsync();

        // ── Publisher mutations ───────────────────────────────────────────────
        public async Task AddPublisherAsync(Publisher publisher) { _db.Publishers.Add(publisher); await _db.SaveChangesAsync(); }
        public async Task UpdatePublisherAsync(Publisher publisher) { _db.Publishers.Update(publisher); await _db.SaveChangesAsync(); }
        public async Task DeletePublisherAsync(Publisher publisher) { _db.Publishers.Remove(publisher); await _db.SaveChangesAsync(); }
        public Task<bool> CanDeletePublisherAsync(int id) =>
            _db.Publishers.Where(p => p.Id == id).Select(p => !p.Games.Any()).FirstOrDefaultAsync();

        // ── Review mutations ──────────────────────────────────────────────────
        public async Task AddReviewAsync(Review review) { _db.Reviews.Add(review); await _db.SaveChangesAsync(); }
        public async Task UpdateReviewAsync(Review review) { _db.Reviews.Update(review); await _db.SaveChangesAsync(); }
        public async Task DeleteReviewAsync(Review review) { _db.Reviews.Remove(review); await _db.SaveChangesAsync(); }

        // ── User mutations ────────────────────────────────────────────────────
        public async Task AddUserAsync(User user) { _db.Users.Add(user); await _db.SaveChangesAsync(); }
        public async Task UpdateUserAsync(User user) { _db.Users.Update(user); await _db.SaveChangesAsync(); }
        public async Task DeleteUserAsync(User user) { _db.Users.Remove(user); await _db.SaveChangesAsync(); }
        public Task<bool> CanDeleteUserAsync(int id) =>
            _db.Users.Where(u => u.Id == id).Select(u => !u.Reviews.Any()).FirstOrDefaultAsync();
    }
}
