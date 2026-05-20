using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class GameTypeController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public GameTypeController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var gameTypes = await _repo.GetAllGameTypesAsync();
            return View(gameTypes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();

            var model = new GameTypeDetailsViewModel
            {
                GameType = gameType,
                Games = gameType.Games.OrderBy(g => g.Name).ToList()
            };

            return View(model);
        }
    }
}
