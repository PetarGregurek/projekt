using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class PublisherController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public PublisherController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var publishers = await _repo.GetAllPublishersAsync();
            return View(publishers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();

            var model = new PublisherDetailsViewModel
            {
                Publisher = publisher,
                Games = publisher.Games.OrderBy(g => g.Name).ToList()
            };

            return View(model);
        }
    }
}
