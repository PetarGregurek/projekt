using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers
{
    public class GameController : Controller
    {
        private readonly IBoardGameRepository _repo;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public GameController(IBoardGameRepository repo, AppDbContext db, IWebHostEnvironment environment)
        {
            _repo = repo;
            _db = db;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var games = await _repo.SearchGamesAsync(search);

            var model = BuildIndexModel(games, search);
            model.StatusMessage = TempData["StatusMessage"] as string;

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_GameTableRows", model);
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var game = await _repo.GetGameWithDetailsAsync(id);
            if (game == null) return NotFound();

            return View(BuildGameDetails(game));
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create()
        {
            var model = await BuildFormViewModelAsync(new GameFormInputModel());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Create(GameFormViewModel model)
        {
            var input = model.Input;
            await ValidateGameInputAsync(input);

            if (!ModelState.IsValid)
            {
                return View(await BuildFormViewModelAsync(input));
            }

            var game = MapToGame(input);
            await _repo.AddGameAsync(game);

            TempData["StatusMessage"] = "Game created successfully.";
            return RedirectToAction(nameof(Details), new { id = game.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _repo.GetGameForEditAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var input = new GameFormInputModel
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                YearPublished = game.YearPublished,
                MinPlayers = game.MinPlayers,
                MaxPlayers = game.MaxPlayers,
                Difficulty = game.Difficulty,
                GameTypeId = game.GameTypeId,
                PublisherId = game.PublisherId,
                CategoryId = game.CategoryId
            };

            return View(await BuildFormViewModelAsync(input));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Edit(int id, GameFormViewModel model)
        {
            var input = model.Input;

            if (id != input.Id)
            {
                return BadRequest();
            }

            await ValidateGameInputAsync(input);

            if (!ModelState.IsValid)
            {
                return View(await BuildFormViewModelAsync(input));
            }

            var game = await _repo.GetGameForEditAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            game.Name = input.Name;
            game.Description = input.Description;
            game.YearPublished = input.YearPublished;
            game.MinPlayers = input.MinPlayers;
            game.MaxPlayers = input.MaxPlayers;
            game.Difficulty = input.Difficulty;
            game.GameTypeId = input.GameTypeId;
            game.PublisherId = input.PublisherId;
            game.CategoryId = input.CategoryId;

            await _repo.UpdateGameAsync(game);

            TempData["StatusMessage"] = "Game updated successfully.";
            return RedirectToAction(nameof(Details), new { id = game.Id });
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _repo.GetGameWithDetailsAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var model = new GameDeleteViewModel
            {
                Game = game,
                CanDelete = await _repo.CanDeleteGameAsync(id),
                ReviewCount = game.Reviews.Count,
                EventCount = game.Events.Count
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _repo.GetGameWithDetailsAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            if (!await _repo.CanDeleteGameAsync(id))
            {
                ModelState.AddModelError(string.Empty, "This game cannot be deleted while related reviews exist.");
                var blockedModel = new GameDeleteViewModel
                {
                    Game = game,
                    CanDelete = false,
                    ReviewCount = game.Reviews.Count,
                    EventCount = game.Events.Count
                };

                return View("Delete", blockedModel);
            }

            await _repo.DeleteGameAsync(game);
            TempData["StatusMessage"] = "Game deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Lookup(string entityType, string? query)
        {
            var normalizedEntityType = entityType?.Trim().ToLowerInvariant();

            return normalizedEntityType switch
            {
                "category" => Json((await _repo.SearchCategoriesAsync(query, 3)).Select(c => new LookupItemViewModel
                {
                    Id = c.Id,
                    Label = c.Name,
                    Description = c.AgeGroup
                })),
                "gametype" => Json((await _repo.SearchGameTypesAsync(query, 3)).Select(t => new LookupItemViewModel
                {
                    Id = t.Id,
                    Label = t.Name,
                    Description = t.Description
                })),
                "publisher" => Json((await _repo.SearchPublishersAsync(query, 3)).Select(p => new LookupItemViewModel
                {
                    Id = p.Id,
                    Label = p.Name,
                    Description = p.Country
                })),
                _ => BadRequest()
            };
        }

        [HttpGet]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> GetFiles(int id)
        {
            var gameExists = await _db.Games.AnyAsync(g => g.Id == id);
            if (!gameExists)
            {
                return NotFound();
            }

            var files = await _db.GameFiles
                .Where(f => f.GameId == id)
                .OrderByDescending(f => f.UploadedAt)
                .Select(f => new
                {
                    f.Id,
                    f.OriginalFileName,
                    f.SizeBytes,
                    f.UploadedAt,
                    f.RelativePath
                })
                .ToListAsync();

            return Json(files);
        }

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> UploadFile(int id, IFormFile? file)
        {
            var gameExists = await _db.Games.AnyAsync(g => g.Id == id);
            if (!gameExists)
            {
                return NotFound(new { message = "Game not found." });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";

            var uploadDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "games",
                id.ToString());

            Directory.CreateDirectory(uploadDirectory);

            var fullPath = Path.Combine(uploadDirectory, storedFileName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/games/{id}/{storedFileName}";

            var gameFile = new GameFile
            {
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow,
                GameId = id
            };

            _db.GameFiles.Add(gameFile);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                gameFile.Id,
                gameFile.OriginalFileName,
                gameFile.SizeBytes,
                gameFile.UploadedAt,
                gameFile.RelativePath
            });
        }

        [HttpDelete]
        [Authorize(Roles = IdentitySeed.AdminRole)]
        public async Task<IActionResult> DeleteFile(int id, int fileId)
        {
            var file = await _db.GameFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.GameId == id);

            if (file == null)
            {
                return NotFound();
            }

            var relativePath = file.RelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _db.GameFiles.Remove(file);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<GameFormViewModel> BuildFormViewModelAsync(GameFormInputModel input)
        {
            var categories = await _repo.GetAllCategoriesAsync();
            var gameTypes = await _repo.GetAllGameTypesAsync();
            var publishers = await _repo.GetAllPublishersAsync();

            return new GameFormViewModel
            {
                Input = input,
                DifficultyOptions = Enum.GetValues<Difficulty>()
                    .Select(d => new SelectListItem(d.ToString(), d.ToString(), d == input.Difficulty))
                    .ToList(),
                CategoryDisplayName = categories.FirstOrDefault(c => c.Id == input.CategoryId)?.Name,
                GameTypeDisplayName = gameTypes.FirstOrDefault(t => t.Id == input.GameTypeId)?.Name,
                PublisherDisplayName = publishers.FirstOrDefault(p => p.Id == input.PublisherId)?.Name,
                StatusMessage = TempData["StatusMessage"] as string
            };
        }

        private async Task ValidateGameInputAsync(GameFormInputModel input)
        {
            if (input.MaxPlayers < input.MinPlayers)
            {
                ModelState.AddModelError("Input.MaxPlayers", "Maximum players must be greater than or equal to minimum players.");
            }

            var categories = await _repo.GetAllCategoriesAsync();
            var gameTypes = await _repo.GetAllGameTypesAsync();
            var publishers = await _repo.GetAllPublishersAsync();

            if (!categories.Any(c => c.Id == input.CategoryId))
            {
                ModelState.AddModelError("Input.CategoryId", "Selected category does not exist.");
            }

            if (!gameTypes.Any(t => t.Id == input.GameTypeId))
            {
                ModelState.AddModelError("Input.GameTypeId", "Selected game type does not exist.");
            }

            if (!publishers.Any(p => p.Id == input.PublisherId))
            {
                ModelState.AddModelError("Input.PublisherId", "Selected publisher does not exist.");
            }
        }

        private static Game MapToGame(GameFormInputModel input) =>
            new()
            {
                Name = input.Name,
                Description = input.Description,
                YearPublished = input.YearPublished,
                MinPlayers = input.MinPlayers,
                MaxPlayers = input.MaxPlayers,
                Difficulty = input.Difficulty,
                GameTypeId = input.GameTypeId,
                PublisherId = input.PublisherId,
                CategoryId = input.CategoryId
            };

        private static GameIndexViewModel BuildIndexModel(List<Game> games, string? search) =>
            new()
            {
                Search = search,
                Games = games
                    .Select(g => new GameListItemViewModel
                    {
                        Game = g,
                        GameTypeName = g.GameType?.Name,
                        PublisherName = g.Publisher?.Name,
                        CategoryName = g.Category?.Name,
                        ReviewCount = g.Reviews.Count,
                        AverageRating = g.Reviews.Count > 0 ? g.Reviews.Average(r => r.Rating) : null,
                        EventCount = g.Events.Count,
                        CanDelete = g.Reviews.Count == 0
                    })
                    .ToList()
            };

        private static GameDetailsViewModel BuildGameDetails(Game game)
        {
            var reviews = game.Reviews
                .Select(r => new ReviewWithUserViewModel { Review = r, User = r.User })
                .OrderByDescending(x => x.Review.CreatedAt)
                .ToList();

            return new GameDetailsViewModel
            {
                Game          = game,
                GameType      = game.GameType,
                Publisher     = game.Publisher,
                Category      = game.Category,
                Reviews       = reviews,
                Events        = game.Events.OrderBy(e => e.StartDateTime).ToList(),
                Files         = game.Files.OrderByDescending(f => f.UploadedAt).ToList(),
                AverageRating = reviews.Count > 0 ? reviews.Average(x => x.Review.Rating) : null
            };
        }
    }
}