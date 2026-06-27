using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class PublisherController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public PublisherController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var publishers = await _repo.SearchPublishersAsync(search);
            var model = new PublisherIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                Publishers = publishers
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_PublisherTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();
            return View(new PublisherDetailsViewModel
            {
                Publisher = publisher,
                Games = publisher.Games.OrderBy(g => g.Name).ToList()
            });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(new PublisherFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(PublisherFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var publisher = new Publisher { Name = model.Input.Name, Country = model.Input.Country };
            await _repo.AddPublisherAsync(publisher);
            TempData["StatusMessage"] = "Publisher created successfully.";
            return RedirectToAction(nameof(Details), new { id = publisher.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();
            return View(new PublisherFormViewModel
            {
                Input = new PublisherFormInputModel { Id = publisher.Id, Name = publisher.Name, Country = publisher.Country }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, PublisherFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();
            publisher.Name = model.Input.Name;
            publisher.Country = model.Input.Country;
            await _repo.UpdatePublisherAsync(publisher);
            TempData["StatusMessage"] = "Publisher updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();
            return View(new PublisherDeleteViewModel
            {
                Publisher = publisher,
                CanDelete = await _repo.CanDeletePublisherAsync(id),
                GameCount = publisher.Games.Count
            });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _repo.GetPublisherWithGamesAsync(id);
            if (publisher == null) return NotFound();
            if (!await _repo.CanDeletePublisherAsync(id))
            {
                TempData["StatusMessage"] = "Cannot delete: publisher has related games.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            await _repo.DeletePublisherAsync(publisher);
            TempData["StatusMessage"] = "Publisher deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
