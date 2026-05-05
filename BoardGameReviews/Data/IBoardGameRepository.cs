using BoardGameReviews.Models;

namespace BoardGameReviews.Data
{
    public interface IBoardGameRepository
    {
        // ── Collections (index) ─────────────────────────────────────────────
        Task<List<Game>>      GetAllGamesAsync();
        Task<List<Category>>  GetAllCategoriesAsync();
        Task<List<GameType>>  GetAllGameTypesAsync();
        Task<List<Publisher>> GetAllPublishersAsync();
        Task<List<User>>      GetAllUsersAsync();
        Task<List<Review>>    GetAllReviewsAsync();
        Task<List<Event>>     GetAllEventsAsync();

        // ── Detail lookups ───────────────────────────────────────────────────
        Task<Game?>      GetGameWithDetailsAsync(int id);
        Task<Category?>  GetCategoryWithGamesAsync(int id);
        Task<GameType?>  GetGameTypeWithGamesAsync(int id);
        Task<Publisher?> GetPublisherWithGamesAsync(int id);
        Task<User?>      GetUserWithReviewsAsync(int id);
        Task<Review?>    GetReviewWithDetailsAsync(int id);
        Task<Event?>     GetEventWithGameAsync(int id);

        // ── Home page ────────────────────────────────────────────────────────
        Task<int> CountGamesAsync();
        Task<int> CountReviewsAsync();
        Task<int> CountUsersAsync();
        Task<int> CountEventsAsync();
        Task<List<Game>>     GetTopRatedGamesAsync(int count);
        Task<List<Event>>    GetUpcomingEventsAsync(int count);
        Task<List<Category>> GetPopularCategoriesAsync(int count);
    }
}
