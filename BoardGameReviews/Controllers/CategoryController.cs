using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public CategoryController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _repo.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();

            var model = new CategoryDetailsViewModel
            {
                Category = category,
                Games = category.Games.OrderBy(g => g.Name).ToList()
            };

            return View(model);
        }
    }
}
