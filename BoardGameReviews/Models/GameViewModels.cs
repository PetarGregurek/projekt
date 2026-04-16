namespace BoardGameReviews.Models
{
    public class GameIndexViewModel
    {
        public List<GameListItemViewModel> Games { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<GameType> GameTypes { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
        public List<EventListItemViewModel> Events { get; set; } = new();
    }

    public class GameListItemViewModel
    {
        public required Game Game { get; set; }
        public string? GameTypeName { get; set; }
        public string? PublisherName { get; set; }
        public string? CategoryName { get; set; }
        public int ReviewCount { get; set; }
        public double? AverageRating { get; set; }
        public int EventCount { get; set; }
    }

    public class ReviewListItemViewModel
    {
        public required Review Review { get; set; }
        public string? GameName { get; set; }
        public string? Username { get; set; }
    }

    public class EventListItemViewModel
    {
        public required Event Event { get; set; }
        public string? GameName { get; set; }
    }

    public class ReviewWithUserViewModel
    {
        public required Review Review { get; set; }
        public User? User { get; set; }
    }

    public class GameDetailsViewModel
    {
        public required Game Game { get; set; }
        public GameType? GameType { get; set; }
        public Publisher? Publisher { get; set; }
        public Category? Category { get; set; }
        public List<ReviewWithUserViewModel> Reviews { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public double? AverageRating { get; set; }
    }

    public class EntityDetailsViewModel
    {
        public string EntityType { get; set; } = "game";
        public int EntityId { get; set; }

        public GameDetailsViewModel? GameDetails { get; set; }

        public Category? Category { get; set; }
        public List<Game> CategoryGames { get; set; } = new();

        public GameType? GameType { get; set; }
        public List<Game> GameTypeGames { get; set; } = new();

        public Publisher? Publisher { get; set; }
        public List<Game> PublisherGames { get; set; } = new();

        public User? User { get; set; }
        public List<ReviewListItemViewModel> UserReviews { get; set; } = new();

        public Review? Review { get; set; }
        public Game? ReviewGame { get; set; }
        public User? ReviewUser { get; set; }

        public Event? Event { get; set; }
        public Game? EventGame { get; set; }
    }
}