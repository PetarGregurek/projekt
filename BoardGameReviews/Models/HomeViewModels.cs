namespace BoardGameReviews.Models
{
    public class HomeIndexViewModel
    {
        public int TotalGames { get; set; }
        public int TotalReviews { get; set; }
        public int TotalUsers { get; set; }
        public int TotalEvents { get; set; }
        public List<HomeTopGameViewModel> TopGames { get; set; } = new();
        public List<HomeEventViewModel> UpcomingEvents { get; set; } = new();
        public List<HomeCategoryViewModel> PopularCategories { get; set; } = new();
    }

    public class HomeTopGameViewModel
    {
        public required Game Game { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? PublisherName { get; set; }
    }

    public class HomeEventViewModel
    {
        public required Event Event { get; set; }
        public string? GameName { get; set; }
    }

    public class HomeCategoryViewModel
    {
        public required Category Category { get; set; }
        public int ReviewCount { get; set; }
    }
}
