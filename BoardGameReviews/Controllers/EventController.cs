using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class EventController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public EventController(IBoardGameRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search)
        {
            var events = await _repo.GetAllEventsAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events.Where(e =>
                    e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Game?.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var model = new EventIndexViewModel
            {
                Search = search,
                StatusMessage = TempData["StatusMessage"] as string,
                Events = events.Select(e => new EventListItemViewModel { Event = e, GameName = e.Game?.Name }).ToList()
            };
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_EventTableRows", model);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var evt = await _repo.GetEventWithGameAsync(id);
            if (evt == null) return NotFound();
            return View(new EventDetailsViewModel { Event = evt, Game = evt.Game });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public IActionResult Create() => View(new EventFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            await ValidateEventInputAsync(model.Input);
            if (!ModelState.IsValid) return View(await RebuildEventFormViewModelAsync(model));
            var evt = new Event
            {
                Name = model.Input.Name,
                GameId = model.Input.GameId,
                StartDateTime = model.Input.StartDateTime!.Value,
                EndDateTime = model.Input.EndDateTime!.Value,
                Location = model.Input.Location
            };
            await _repo.AddEventAsync(evt);
            TempData["StatusMessage"] = "Event created successfully.";
            return RedirectToAction(nameof(Details), new { id = evt.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var evt = await _repo.GetEventWithGameAsync(id);
            if (evt == null) return NotFound();
            var games = await _repo.GetAllGamesAsync();
            return View(new EventFormViewModel
            {
                Input = new EventFormInputModel
                {
                    Id = evt.Id, Name = evt.Name, GameId = evt.GameId,
                    StartDateTime = evt.StartDateTime, EndDateTime = evt.EndDateTime, Location = evt.Location
                },
                GameDisplayName = games.FirstOrDefault(g => g.Id == evt.GameId)?.Name
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model)
        {
            if (id != model.Input.Id) return BadRequest();
            await ValidateEventInputAsync(model.Input);
            if (!ModelState.IsValid) return View(await RebuildEventFormViewModelAsync(model));
            var evt = await _repo.GetEventWithGameAsync(id);
            if (evt == null) return NotFound();
            evt.Name = model.Input.Name;
            evt.GameId = model.Input.GameId;
            evt.StartDateTime = model.Input.StartDateTime!.Value;
            evt.EndDateTime = model.Input.EndDateTime!.Value;
            evt.Location = model.Input.Location;
            await _repo.UpdateEventAsync(evt);
            TempData["StatusMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _repo.GetEventWithGameAsync(id);
            if (evt == null) return NotFound();
            return View(new EventDeleteViewModel { Event = evt, CanDelete = true, GameName = evt.Game?.Name });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evt = await _repo.GetEventWithGameAsync(id);
            if (evt == null) return NotFound();
            await _repo.DeleteEventAsync(evt);
            TempData["StatusMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Lookup(string? query)
        {
            var games = await _repo.SearchGamesAsync(query, take: 3);
            return Json(games.Select(g => new LookupItemViewModel { Id = g.Id, Label = g.Name, Description = g.YearPublished.ToString() }));
        }

        private async Task ValidateEventInputAsync(EventFormInputModel input)
        {
            if (input.StartDateTime.HasValue && input.EndDateTime.HasValue &&
                input.EndDateTime.Value <= input.StartDateTime.Value)
                ModelState.AddModelError("Input.EndDateTime", "End date/time must be after start date/time.");
            var games = await _repo.GetAllGamesAsync();
            if (!games.Any(g => g.Id == input.GameId))
                ModelState.AddModelError("Input.GameId", "Selected game does not exist.");
        }

        private async Task<EventFormViewModel> RebuildEventFormViewModelAsync(EventFormViewModel model)
        {
            var games = await _repo.GetAllGamesAsync();
            model.GameDisplayName = games.FirstOrDefault(g => g.Id == model.Input.GameId)?.Name;
            return model;
        }
    }
}
