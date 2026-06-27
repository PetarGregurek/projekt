using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class GameTypeController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public GameTypeController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var gameTypes = await _repo.SearchGameTypesAsync(search);
            var model = new GameTypeIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                GameTypes = gameTypes
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_GameTypeTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();
            return View(new GameTypeDetailsViewModel
            {
                GameType = gameType,
                Games = gameType.Games.OrderBy(g => g.Name).ToList()
            });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(new GameTypeFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(GameTypeFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var gameType = new GameType { Name = model.Input.Name, Description = model.Input.Description };
            await _repo.AddGameTypeAsync(gameType);
            TempData["StatusMessage"] = "Game type created successfully.";
            return RedirectToAction(nameof(Details), new { id = gameType.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();
            return View(new GameTypeFormViewModel
            {
                Input = new GameTypeFormInputModel { Id = gameType.Id, Name = gameType.Name, Description = gameType.Description }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, GameTypeFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();
            gameType.Name = model.Input.Name;
            gameType.Description = model.Input.Description;
            await _repo.UpdateGameTypeAsync(gameType);
            TempData["StatusMessage"] = "Game type updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();
            return View(new GameTypeDeleteViewModel
            {
                GameType = gameType,
                CanDelete = await _repo.CanDeleteGameTypeAsync(id),
                GameCount = gameType.Games.Count
            });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gameType = await _repo.GetGameTypeWithGamesAsync(id);
            if (gameType == null) return NotFound();
            if (!await _repo.CanDeleteGameTypeAsync(id))
            {
                TempData["StatusMessage"] = "Cannot delete: game type has related games.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            await _repo.DeleteGameTypeAsync(gameType);
            TempData["StatusMessage"] = "Game type deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
