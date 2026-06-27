using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers;

public class SearchController : Controller
{
    private const int MaxResultsPerGroup = 8;

    private readonly AppDbContext _db;
    private readonly ILogger<SearchController> _logger;

    public SearchController(AppDbContext db, ILogger<SearchController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = q?.Trim() ?? string.Empty;
        var model = new GlobalSearchViewModel { Query = query };

        model.Groups.Add(BuildPagesGroup(query));

        if (!string.IsNullOrWhiteSpace(query))
        {
            model.Groups.Add(await SearchGamesAsync(query));
            model.Groups.Add(await SearchCategoriesAsync(query));
            model.Groups.Add(await SearchGameTypesAsync(query));
            model.Groups.Add(await SearchPublishersAsync(query));
            model.Groups.Add(await SearchUsersAsync(query));
            model.Groups.Add(await SearchReviewsAsync(query));
            model.Groups.Add(await SearchEventsAsync(query));

            _logger.LogInformation(
                "Global search executed for query {Query}. Total results: {TotalResults}",
                query,
                model.TotalResults);
        }

        model.Groups = model.Groups
            .Where(group => group.Results.Count > 0)
            .ToList();

        return View(model);
    }

    private GlobalSearchGroupViewModel BuildPagesGroup(string query)
    {
        var pages = new[]
        {
            new PageSearchItem("Home", "Application dashboard and homepage", "Page", Url.Action("Index", "Home"), "home dashboard boardgamereviews"),
            new PageSearchItem("Privacy", "Privacy information", "Page", Url.Action("Privacy", "Home"), "privacy policy"),
            new PageSearchItem("Games", "Browse, create, update, and delete board games", "Menu", Url.Action("Index", "Game"), "games board games igra igre"),
            new PageSearchItem("Categories", "Browse game categories", "Menu", Url.Action("Index", "Category"), "categories category kategorije"),
            new PageSearchItem("Types", "Browse game types", "Menu", Url.Action("Index", "GameType"), "types game types tipovi"),
            new PageSearchItem("Publishers", "Browse publishers", "Menu", Url.Action("Index", "Publisher"), "publishers publisher izdavaci izdavac"),
            new PageSearchItem("Users", "Browse users", "Menu", Url.Action("Index", "User"), "users korisnici"),
            new PageSearchItem("Reviews", "Browse reviews and ratings", "Menu", Url.Action("Index", "Review"), "reviews ratings recenzije ocjene"),
            new PageSearchItem("Events", "Browse game events", "Menu", Url.Action("Index", "Event"), "events dogadaji event")
        };

        var normalizedQuery = query.Trim();
        var matches = string.IsNullOrWhiteSpace(normalizedQuery)
            ? pages
            : pages.Where(page =>
                Contains(page.Title, normalizedQuery) ||
                Contains(page.Description, normalizedQuery) ||
                Contains(page.Keywords, normalizedQuery));

        return new GlobalSearchGroupViewModel
        {
            Name = "Pages and menu",
            Results = matches
                .Select(page => new GlobalSearchResultViewModel
                {
                    Title = page.Title,
                    Description = page.Description,
                    Type = page.Type,
                    Url = page.Url ?? "#"
                })
                .ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchGamesAsync(string query)
    {
        var results = await _db.Games
            .Include(game => game.Category)
            .Include(game => game.GameType)
            .Include(game => game.Publisher)
            .Where(game =>
                game.Name.Contains(query) ||
                (game.Description != null && game.Description.Contains(query)) ||
                (game.Category != null && game.Category.Name.Contains(query)) ||
                (game.GameType != null && game.GameType.Name.Contains(query)) ||
                (game.Publisher != null && game.Publisher.Name.Contains(query)))
            .OrderBy(game => game.Name)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Games",
            Results = results.Select(game => new GlobalSearchResultViewModel
            {
                Title = game.Name,
                Description = $"{game.YearPublished} · {game.Category?.Name ?? "No category"} · {game.Publisher?.Name ?? "No publisher"}",
                Type = "Game",
                Url = Url.Action("Details", "Game", new { id = game.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchCategoriesAsync(string query)
    {
        var results = await _db.Categories
            .Where(category =>
                category.Name.Contains(query) ||
                (category.Description != null && category.Description.Contains(query)) ||
                (category.AgeGroup != null && category.AgeGroup.Contains(query)))
            .OrderBy(category => category.Name)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Categories",
            Results = results.Select(category => new GlobalSearchResultViewModel
            {
                Title = category.Name,
                Description = category.Description,
                Type = "Category",
                Url = Url.Action("Details", "Category", new { id = category.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchGameTypesAsync(string query)
    {
        var results = await _db.GameTypes
            .Where(type =>
                type.Name.Contains(query) ||
                (type.Description != null && type.Description.Contains(query)))
            .OrderBy(type => type.Name)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Types",
            Results = results.Select(type => new GlobalSearchResultViewModel
            {
                Title = type.Name,
                Description = type.Description,
                Type = "Type",
                Url = Url.Action("Details", "GameType", new { id = type.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchPublishersAsync(string query)
    {
        var results = await _db.Publishers
            .Where(publisher =>
                publisher.Name.Contains(query) ||
                (publisher.Country != null && publisher.Country.Contains(query)))
            .OrderBy(publisher => publisher.Name)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Publishers",
            Results = results.Select(publisher => new GlobalSearchResultViewModel
            {
                Title = publisher.Name,
                Description = publisher.Country,
                Type = "Publisher",
                Url = Url.Action("Details", "Publisher", new { id = publisher.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchUsersAsync(string query)
    {
        var results = await _db.Users
            .Where(user =>
                user.Username.Contains(query) ||
                user.Email.Contains(query) ||
                (user.Country != null && user.Country.Contains(query)))
            .OrderBy(user => user.Username)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Users",
            Results = results.Select(user => new GlobalSearchResultViewModel
            {
                Title = user.Username,
                Description = $"{user.Email} · {user.Country ?? "No country"}",
                Type = "User",
                Url = Url.Action("Details", "User", new { id = user.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchReviewsAsync(string query)
    {
        var results = await _db.Reviews
            .Include(review => review.Game)
            .Include(review => review.User)
            .Where(review =>
                review.Title.Contains(query) ||
                (review.Comment != null && review.Comment.Contains(query)) ||
                (review.Game != null && review.Game.Name.Contains(query)) ||
                (review.User != null && review.User.Username.Contains(query)))
            .OrderByDescending(review => review.CreatedAt)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Reviews",
            Results = results.Select(review => new GlobalSearchResultViewModel
            {
                Title = review.Title,
                Description = $"{review.Rating}/5 · {review.Game?.Name ?? "No game"} · {review.User?.Username ?? "No user"}",
                Type = "Review",
                Url = Url.Action("Details", "Review", new { id = review.Id }) ?? "#"
            }).ToList()
        };
    }

    private async Task<GlobalSearchGroupViewModel> SearchEventsAsync(string query)
    {
        var results = await _db.Events
            .Include(gameEvent => gameEvent.Game)
            .Where(gameEvent =>
                gameEvent.Name.Contains(query) ||
                gameEvent.Location.Contains(query) ||
                (gameEvent.Game != null && gameEvent.Game.Name.Contains(query)))
            .OrderBy(gameEvent => gameEvent.StartDateTime)
            .Take(MaxResultsPerGroup)
            .ToListAsync();

        return new GlobalSearchGroupViewModel
        {
            Name = "Events",
            Results = results.Select(gameEvent => new GlobalSearchResultViewModel
            {
                Title = gameEvent.Name,
                Description = $"{gameEvent.StartDateTime:g} · {gameEvent.Location}",
                Type = "Event",
                Url = Url.Action("Details", "Event", new { id = gameEvent.Id }) ?? "#"
            }).ToList()
        };
    }

    private static bool Contains(string? value, string query) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private sealed record PageSearchItem(
        string Title,
        string Description,
        string Type,
        string? Url,
        string Keywords);
}
