using System.Diagnostics;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<Game> _games;
        private readonly List<Review> _reviews;
        private readonly List<User> _users;
        private readonly List<Event> _events;
        private readonly List<Category> _categories;
        private readonly List<Publisher> _publishers;

        public HomeController(
            ILogger<HomeController> logger,
            List<Game> games,
            List<Review> reviews,
            List<User> users,
            List<Event> events,
            List<Category> categories,
            List<Publisher> publishers)
        {
            _logger = logger;
            _games = games;
            _reviews = reviews;
            _users = users;
            _events = events;
            _categories = categories;
            _publishers = publishers;
        }

        public IActionResult Index()
        {
            var topGames = _games
                .Select(g =>
                {
                    var gameReviews = _reviews.Where(r => r.GameId == g.Id).ToList();
                    var averageRating = gameReviews.Count > 0 ? gameReviews.Average(r => r.Rating) : 0;

                    return new HomeTopGameViewModel
                    {
                        Game = g,
                        AverageRating = averageRating,
                        ReviewCount = gameReviews.Count,
                        PublisherName = _publishers.FirstOrDefault(p => p.Id == g.PublisherId)?.Name
                    };
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(3)
                .ToList();

            var upcomingEvents = _events
                .Where(e => e.EndDateTime >= DateTime.Now)
                .OrderBy(e => e.StartDateTime)
                .Take(4)
                .Select(e => new HomeEventViewModel
                {
                    Event = e,
                    GameName = _games.FirstOrDefault(g => g.Id == e.GameId)?.Name
                })
                .ToList();

            var popularCategories = _categories
                .Select(c => new HomeCategoryViewModel
                {
                    Category = c,
                    ReviewCount = _reviews.Count(r => _games.Any(g => g.Id == r.GameId && g.CategoryId == c.Id))
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(3)
                .ToList();

            var model = new HomeIndexViewModel
            {
                TotalGames = _games.Count,
                TotalReviews = _reviews.Count,
                TotalUsers = _users.Count,
                TotalEvents = _events.Count,
                TopGames = topGames,
                UpcomingEvents = upcomingEvents,
                PopularCategories = popularCategories
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
