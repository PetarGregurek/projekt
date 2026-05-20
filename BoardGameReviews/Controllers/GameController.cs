using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class GameController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public GameController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _repo.GetAllGamesAsync();

            var model = new GameIndexViewModel
            {
                Games = games
                    .Select(g => new GameListItemViewModel
                    {
                        Game          = g,
                        GameTypeName  = g.GameType?.Name,
                        PublisherName = g.Publisher?.Name,
                        CategoryName  = g.Category?.Name,
                        ReviewCount   = g.Reviews.Count,
                        AverageRating = g.Reviews.Count > 0 ? g.Reviews.Average(r => r.Rating) : null,
                        EventCount    = g.Events.Count
                    })
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var game = await _repo.GetGameWithDetailsAsync(id);
            if (game == null) return NotFound();

            return View(BuildGameDetails(game));
        }

        private static GameDetailsViewModel BuildGameDetails(Game game)
        {
            var reviews = game.Reviews
                .Select(r => new ReviewWithUserViewModel { Review = r, User = r.User })
                .OrderByDescending(x => x.Review.CreatedAt)
                .ToList();

            return new GameDetailsViewModel
            {
                Game          = game,
                GameType      = game.GameType,
                Publisher     = game.Publisher,
                Category      = game.Category,
                Reviews       = reviews,
                Events        = game.Events.OrderBy(e => e.StartDateTime).ToList(),
                AverageRating = reviews.Count > 0 ? reviews.Average(x => x.Review.Rating) : null
            };
        }
    }
}