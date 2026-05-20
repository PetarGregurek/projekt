using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class UserController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public UserController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _repo.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();

            var model = new UserDetailsViewModel
            {
                User = user,
                Reviews = user.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewListItemViewModel
                    {
                        Review = r,
                        GameName = r.Game?.Name,
                        Username = user.Username
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}
