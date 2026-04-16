using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    public class GameController : Controller
    {
        private readonly List<Game> _games;
        private readonly List<GameType> _gameTypes;
        private readonly List<Publisher> _publishers;
        private readonly List<Category> _categories;
        private readonly List<Review> _reviews;
        private readonly List<User> _users;
        private readonly List<Event> _events;

        public GameController(
            List<Game> games,
            List<GameType> gameTypes,
            List<Publisher> publishers,
            List<Category> categories,
            List<Review> reviews,
            List<User> users,
            List<Event> events)
        {
            _games = games;
            _gameTypes = gameTypes;
            _publishers = publishers;
            _categories = categories;
            _reviews = reviews;
            _users = users;
            _events = events;
        }

        public IActionResult Index()
        {
            var model = new GameIndexViewModel
            {
                Games = _games
                    .Select(g =>
                    {
                        var gameReviews = _reviews.Where(r => r.GameId == g.Id).ToList();

                        return new GameListItemViewModel
                        {
                            Game = g,
                            GameTypeName = _gameTypes.FirstOrDefault(t => t.Id == g.GameTypeId)?.Name,
                            PublisherName = _publishers.FirstOrDefault(p => p.Id == g.PublisherId)?.Name,
                            CategoryName = _categories.FirstOrDefault(c => c.Id == g.CategoryId)?.Name,
                            ReviewCount = gameReviews.Count,
                            AverageRating = gameReviews.Count > 0 ? gameReviews.Average(r => r.Rating) : null,
                            EventCount = _events.Count(e => e.GameId == g.Id)
                        };
                    })
                    .OrderBy(x => x.Game.Name)
                    .ToList(),
                Categories = _categories.OrderBy(c => c.Name).ToList(),
                GameTypes = _gameTypes.OrderBy(t => t.Name).ToList(),
                Publishers = _publishers.OrderBy(p => p.Name).ToList(),
                Users = _users.OrderBy(u => u.Username).ToList(),
                Reviews = _reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewListItemViewModel
                    {
                        Review = r,
                        GameName = _games.FirstOrDefault(g => g.Id == r.GameId)?.Name,
                        Username = _users.FirstOrDefault(u => u.Id == r.UserId)?.Username
                    })
                    .ToList(),
                Events = _events
                    .OrderBy(e => e.StartDateTime)
                    .Select(e => new EventListItemViewModel
                    {
                        Event = e,
                        GameName = _games.FirstOrDefault(g => g.Id == e.GameId)?.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Details(int id, string entity = "game")
        {
            var normalizedEntity = (entity ?? "game").Trim().ToLowerInvariant();
            var model = new EntityDetailsViewModel
            {
                EntityType = normalizedEntity,
                EntityId = id
            };

            switch (normalizedEntity)
            {
                case "game":
                {
                    var game = _games.FirstOrDefault(g => g.Id == id);
                    if (game == null)
                    {
                        return NotFound();
                    }

                    model.GameDetails = BuildGameDetails(game);
                    break;
                }
                case "category":
                {
                    model.Category = _categories.FirstOrDefault(c => c.Id == id);
                    if (model.Category == null)
                    {
                        return NotFound();
                    }

                    model.CategoryGames = _games
                        .Where(g => g.CategoryId == id)
                        .OrderBy(g => g.Name)
                        .ToList();
                    break;
                }
                case "gametype":
                {
                    model.GameType = _gameTypes.FirstOrDefault(t => t.Id == id);
                    if (model.GameType == null)
                    {
                        return NotFound();
                    }

                    model.GameTypeGames = _games
                        .Where(g => g.GameTypeId == id)
                        .OrderBy(g => g.Name)
                        .ToList();
                    break;
                }
                case "publisher":
                {
                    model.Publisher = _publishers.FirstOrDefault(p => p.Id == id);
                    if (model.Publisher == null)
                    {
                        return NotFound();
                    }

                    model.PublisherGames = _games
                        .Where(g => g.PublisherId == id)
                        .OrderBy(g => g.Name)
                        .ToList();
                    break;
                }
                case "user":
                {
                    model.User = _users.FirstOrDefault(u => u.Id == id);
                    if (model.User == null)
                    {
                        return NotFound();
                    }

                    model.UserReviews = _reviews
                        .Where(r => r.UserId == id)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new ReviewListItemViewModel
                        {
                            Review = r,
                            GameName = _games.FirstOrDefault(g => g.Id == r.GameId)?.Name,
                            Username = model.User.Username
                        })
                        .ToList();
                    break;
                }
                case "review":
                {
                    model.Review = _reviews.FirstOrDefault(r => r.Id == id);
                    if (model.Review == null)
                    {
                        return NotFound();
                    }

                    model.ReviewGame = _games.FirstOrDefault(g => g.Id == model.Review.GameId);
                    model.ReviewUser = _users.FirstOrDefault(u => u.Id == model.Review.UserId);
                    break;
                }
                case "event":
                {
                    model.Event = _events.FirstOrDefault(e => e.Id == id);
                    if (model.Event == null)
                    {
                        return NotFound();
                    }

                    model.EventGame = _games.FirstOrDefault(g => g.Id == model.Event.GameId);
                    break;
                }
                default:
                    return NotFound();
            }

            return View(model);
        }

        private GameDetailsViewModel BuildGameDetails(Game game)
        {
            var gameReviews = _reviews
                .Where(r => r.GameId == game.Id)
                .Select(r => new ReviewWithUserViewModel
                {
                    Review = r,
                    User = _users.FirstOrDefault(u => u.Id == r.UserId)
                })
                .OrderByDescending(x => x.Review.CreatedAt)
                .ToList();

            var gameEvents = _events
                .Where(e => e.GameId == game.Id)
                .OrderBy(e => e.StartDateTime)
                .ToList();

            return new GameDetailsViewModel
            {
                Game = game,
                GameType = _gameTypes.FirstOrDefault(t => t.Id == game.GameTypeId),
                Publisher = _publishers.FirstOrDefault(p => p.Id == game.PublisherId),
                Category = _categories.FirstOrDefault(c => c.Id == game.CategoryId),
                Reviews = gameReviews,
                Events = gameEvents,
                AverageRating = gameReviews.Count > 0 ? gameReviews.Average(x => x.Review.Rating) : null
            };
        }
    }
}