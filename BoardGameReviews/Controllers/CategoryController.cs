using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoardGameReviews.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public CategoryController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var categories = await _repo.SearchCategoriesAsync(search);
            var model = new CategoryIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                Categories = categories
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_CategoryTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();
            return View(new CategoryDetailsViewModel
            {
                Category = category,
                Games = category.Games.OrderBy(g => g.Name).ToList()
            });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(BuildFormViewModel(new CategoryFormInputModel()));

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View(BuildFormViewModel(model.Input));
            var category = new Category
            {
                Name = model.Input.Name,
                Description = model.Input.Description,
                AgeGroup = model.Input.AgeGroup,
                Difficulty = model.Input.Difficulty,
                Popularity = model.Input.Popularity
            };
            await _repo.AddCategoryAsync(category);
            TempData["StatusMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Details), new { id = category.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();
            return View(BuildFormViewModel(new CategoryFormInputModel
            {
                Id = category.Id, Name = category.Name, Description = category.Description,
                AgeGroup = category.AgeGroup, Difficulty = category.Difficulty, Popularity = category.Popularity
            }));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            if (!ModelState.IsValid) return View(BuildFormViewModel(model.Input));
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();
            category.Name = model.Input.Name;
            category.Description = model.Input.Description;
            category.AgeGroup = model.Input.AgeGroup;
            category.Difficulty = model.Input.Difficulty;
            category.Popularity = model.Input.Popularity;
            await _repo.UpdateCategoryAsync(category);
            TempData["StatusMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();
            return View(new CategoryDeleteViewModel
            {
                Category = category,
                CanDelete = await _repo.CanDeleteCategoryAsync(id),
                GameCount = category.Games.Count
            });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _repo.GetCategoryWithGamesAsync(id);
            if (category == null) return NotFound();
            if (!await _repo.CanDeleteCategoryAsync(id))
            {
                TempData["StatusMessage"] = "Cannot delete: category has related games.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            await _repo.DeleteCategoryAsync(category);
            TempData["StatusMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private CategoryFormViewModel BuildFormViewModel(CategoryFormInputModel input) => new()
        {
            Input = input,
            DifficultyOptions = Enum.GetValues<Difficulty>()
                .Select(d => new SelectListItem(d.ToString(), d.ToString(), d == input.Difficulty))
                .ToList()
        };
    }
}
