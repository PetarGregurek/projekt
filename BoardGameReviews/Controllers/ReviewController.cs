using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public ReviewController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var reviews = await _repo.GetAllReviewsAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                reviews = reviews.Where(r =>
                    r.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (r.Game?.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.User?.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            var model = new ReviewIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                Reviews = reviews.Select(r => new ReviewListItemViewModel
                {
                    Review = r, GameName = r.Game?.Name, Username = r.User?.Username
                }).ToList()
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_ReviewTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();
            return View(new ReviewDetailsViewModel { Review = review, Game = review.Game, User = review.User });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(new ReviewFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(ReviewFormViewModel model)
        {
            await ValidateReviewInputAsync(model.Input);
            if (!ModelState.IsValid) return View(await RebuildFormAsync(model));
            var review = new Review
            {
                Rating = model.Input.Rating,
                Title = model.Input.Title,
                Comment = model.Input.Comment,
                IsRecommended = model.Input.IsRecommended,
                CreatedAt = model.Input.CreatedAt ?? DateTime.Now,
                GameId = model.Input.GameId,
                UserId = model.Input.UserId
            };
            await _repo.AddReviewAsync(review);
            TempData["StatusMessage"] = "Review created successfully.";
            return RedirectToAction(nameof(Details), new { id = review.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();
            var games = await _repo.GetAllGamesAsync();
            var users = await _repo.GetAllUsersAsync();
            return View(new ReviewFormViewModel
            {
                Input = new ReviewFormInputModel
                {
                    Id = review.Id, Rating = review.Rating, Title = review.Title, Comment = review.Comment,
                    IsRecommended = review.IsRecommended, CreatedAt = review.CreatedAt,
                    GameId = review.GameId, UserId = review.UserId
                },
                GameDisplayName = games.FirstOrDefault(g => g.Id == review.GameId)?.Name,
                UserDisplayName = users.FirstOrDefault(u => u.Id == review.UserId)?.Username
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, ReviewFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            await ValidateReviewInputAsync(model.Input);
            if (!ModelState.IsValid) return View(await RebuildFormAsync(model));
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();
            review.Rating = model.Input.Rating;
            review.Title = model.Input.Title;
            review.Comment = model.Input.Comment;
            review.IsRecommended = model.Input.IsRecommended;
            review.CreatedAt = model.Input.CreatedAt ?? DateTime.Now;
            review.GameId = model.Input.GameId;
            review.UserId = model.Input.UserId;
            await _repo.UpdateReviewAsync(review);
            TempData["StatusMessage"] = "Review updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();
            return View(new ReviewDeleteViewModel
            {
                Review = review, CanDelete = true,
                GameName = review.Game?.Name, Username = review.User?.Username
            });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _repo.GetReviewWithDetailsAsync(id);
            if (review == null) return NotFound();
            await _repo.DeleteReviewAsync(review);
            TempData["StatusMessage"] = "Review deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Lookup(string entityType, string? query)
        {
            return entityType switch
            {
                "game" => Json((await _repo.SearchGamesAsync(query, take: 3))
                    .Select(g => new LookupItemViewModel { Id = g.Id, Label = g.Name, Description = g.YearPublished.ToString() })),
                "user" => Json((await _repo.SearchUsersAsync(query, take: 3))
                    .Select(u => new LookupItemViewModel { Id = u.Id, Label = u.Username, Description = u.Email })),
                _ => BadRequest()
            };
        }

        private async Task ValidateReviewInputAsync(ReviewFormInputModel input)
        {
            var games = await _repo.GetAllGamesAsync();
            if (!games.Any(g => g.Id == input.GameId))
                ModelState.AddModelError("Input.GameId", "Selected game does not exist.");
            var users = await _repo.GetAllUsersAsync();
            if (!users.Any(u => u.Id == input.UserId))
                ModelState.AddModelError("Input.UserId", "Selected user does not exist.");
        }

        private async Task<ReviewFormViewModel> RebuildFormAsync(ReviewFormViewModel model)
        {
            var games = await _repo.GetAllGamesAsync();
            var users = await _repo.GetAllUsersAsync();
            model.GameDisplayName = games.FirstOrDefault(g => g.Id == model.Input.GameId)?.Name;
            model.UserDisplayName = users.FirstOrDefault(u => u.Id == model.Input.UserId)?.Username;
            return model;
        }
    }
}
