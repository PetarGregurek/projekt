using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public ReviewController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _repo.GetAllReviewsAsync();
            var model = reviews
                .Select(r => new ReviewListItemViewModel
                {
                    Review = r,
                    GameName = r.Game?.Name,
                    Username = r.User?.Username
                })
                .ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();

            var model = new ReviewDetailsViewModel
            {
                Review = review,
                Game = review.Game,
                User = review.User
            };

            return View(model);
        }
    }
}
