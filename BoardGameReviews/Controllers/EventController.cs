using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class EventController : Controller
    {
        private readonly IBoardGameRepository _repo;

        public EventController(IBoardGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _repo.GetAllEventsAsync();
            var model = events
                .Select(e => new EventListItemViewModel
                {
                    Event = e,
                    GameName = e.Game?.Name
                })
                .ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _repo.GetEventWithGameAsync(id);
            if (eventEntity == null) return NotFound();

            var model = new EventDetailsViewModel
            {
                Event = eventEntity,
                Game = eventEntity.Game
            };

            return View(model);
        }
    }
}
