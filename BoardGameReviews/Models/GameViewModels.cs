namespace BoardGameReviews.Models
{
    public class GameIndexViewModel
    {
        public List<GameListItemViewModel> Games { get; set; } = new();
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

    public class CategoryDetailsViewModel
    {
        public required Category Category { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class GameTypeDetailsViewModel
    {
        public required GameType GameType { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class PublisherDetailsViewModel
    {
        public required Publisher Publisher { get; set; }
        public List<Game> Games { get; set; } = new();
    }

    public class UserDetailsViewModel
    {
        public required User User { get; set; }
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
    }

    public class ReviewDetailsViewModel
    {
        public required Review Review { get; set; }
        public Game? Game { get; set; }
        public User? User { get; set; }
    }

    public class EventDetailsViewModel
    {
        public required Event Event { get; set; }
        public Game? Game { get; set; }
    }
}