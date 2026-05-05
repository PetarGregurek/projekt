using System.Diagnostics;
using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBoardGameRepository _repo;

        public HomeController(ILogger<HomeController> logger, IBoardGameRepository repo)
        {
            _logger = logger;
            _repo   = repo;
        }

        public async Task<IActionResult> Index()
        {
            var topGames          = await _repo.GetTopRatedGamesAsync(3);
            var upcomingEvents    = await _repo.GetUpcomingEventsAsync(4);
            var popularCategories = await _repo.GetPopularCategoriesAsync(3);

            var model = new HomeIndexViewModel
            {
                TotalGames    = await _repo.CountGamesAsync(),
                TotalReviews  = await _repo.CountReviewsAsync(),
                TotalUsers    = await _repo.CountUsersAsync(),
                TotalEvents   = await _repo.CountEventsAsync(),
                TopGames = topGames
                    .Select(g => new HomeTopGameViewModel
                    {
                        Game          = g,
                        AverageRating = g.Reviews.Count > 0 ? g.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount   = g.Reviews.Count,
                        PublisherName = g.Publisher?.Name
                    })
                    .ToList(),
                UpcomingEvents = upcomingEvents
                    .Select(e => new HomeEventViewModel
                    {
                        Event    = e,
                        GameName = e.Game?.Name
                    })
                    .ToList(),
                PopularCategories = popularCategories
                    .Select(c => new HomeCategoryViewModel
                    {
                        Category    = c,
                        ReviewCount = c.Games.Sum(g => g.Reviews.Count)
                    })
                    .ToList()
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
