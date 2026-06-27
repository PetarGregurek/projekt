using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class UserController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public UserController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var users = await _repo.SearchUsersAsync(search);
            var model = new UserIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                Users = users
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_UserTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();
            return View(new UserDetailsViewModel
            {
                User = user,
                Reviews = user.Reviews.OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewListItemViewModel { Review = r, GameName = r.Game?.Name, Username = user.Username })
                    .ToList()
            });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(new UserFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = new User
            {
                Username = model.Input.Username,
                Email = model.Input.Email,
                HashedPassword = model.Input.Password ?? string.Empty,
                Country = model.Input.Country,
                Age = model.Input.Age
            };
            await _repo.AddUserAsync(user);
            TempData["StatusMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();
            return View(new UserFormViewModel
            {
                Input = new UserFormInputModel
                {
                    Id = user.Id, Username = user.Username, Email = user.Email,
                    Country = user.Country, Age = user.Age
                }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();
            user.Username = model.Input.Username;
            user.Email = model.Input.Email;
            if (!string.IsNullOrWhiteSpace(model.Input.Password))
                user.HashedPassword = model.Input.Password;
            user.Country = model.Input.Country;
            user.Age = model.Input.Age;
            await _repo.UpdateUserAsync(user);
            TempData["StatusMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();
            return View(new UserDeleteViewModel
            {
                User = user,
                CanDelete = await _repo.CanDeleteUserAsync(id),
                ReviewCount = user.Reviews.Count
            });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _repo.GetUserWithReviewsAsync(id);
            if (user == null) return NotFound();
            if (!await _repo.CanDeleteUserAsync(id))
            {
                TempData["StatusMessage"] = "Cannot delete: user has related reviews.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            await _repo.DeleteUserAsync(user);
            TempData["StatusMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
