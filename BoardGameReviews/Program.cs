using BoardGameReviews.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ResolveContentRootPath()
});

builder.Services.AddControllersWithViews();


var categories = SampleData.Categories;
var gameTypes = SampleData.GameTypes;
var publishers = SampleData.Publishers;
var games = SampleData.Games;
var users = SampleData.Users;
var reviews = SampleData.Reviews;
var events = SampleData.Events;

var upcomingEvents = events
    .Where(e => e.StartDateTime > DateTime.Now)
    .OrderBy(e => e.StartDateTime)
    .ToList();

var recentEvents = events
    .Where(e => e.StartDateTime > DateTime.Now.AddMonths(-1))
    .OrderByDescending(e => e.StartDateTime)
    .ToList();

var recentReviews = reviews
    .Where(r => r.CreatedAt > DateTime.Now.AddMonths(-1))
    .OrderByDescending(r => r.CreatedAt)
    .ToList();

var topRatedGames = games
    .Select(g => new
    {
        Game = g,
        AverageRating = reviews.Where(r => r.GameId == g.Id).Average(r => r.Rating)
    })
    .OrderByDescending(g => g.AverageRating)
    .Take(5)
    .ToList();

var popularCategories = categories
    .Select(c => new
    {
        Category = c,
        Popularity = reviews.Count(r => games.Any(g => g.Id == r.GameId && g.CategoryId == c.Id))
    })
    .OrderByDescending(c => c.Popularity)
    .ToList();

var activeUsers = users
    .Select(u => new
    {
        User = u,
        ReviewCount = reviews.Count(r => r.UserId == u.Id)
    })
    .OrderByDescending(u => u.ReviewCount)
    .Take(5)
    .ToList();

var recommendedGames = games
    .Where(g => reviews.Any(r => r.GameId == g.Id && r.IsRecommended))
    .ToList();

var gamesByPublisher = publishers
    .Select(p => new
    {
        Publisher = p,
        Games = games.Where(g => g.PublisherId == p.Id).ToList()
    })
    .ToList();

var gamesByCategory = categories
    .Select(c => new
    {
        Category = c,
        Games = games.Where(g => g.CategoryId == c.Id).ToList()
    })
    .ToList();

var gamesByDifficulty = Enum.GetValues(typeof(Difficulty))
    .Cast<Difficulty>()
    .Select(d => new
    {
        Difficulty = d,
        Games = games.Where(g => g.Difficulty == d).ToList()
    })
    .ToList();

var gamesByPlayerCount = games
    .GroupBy(g => new { g.MinPlayers, g.MaxPlayers })
    .Select(g => new
    {
        PlayerCountRange = $"{g.Key.MinPlayers}-{g.Key.MaxPlayers} players",
        Games = g.ToList()
    })
    .ToList();

var gamesByYear = games
    .GroupBy(g => g.YearPublished)
    .Select(g => new
    {
        Year = g.Key,
        Games = g.ToList()
    })
    .ToList();

var gamesByType = gameTypes
    .Select(t => new
    {
        GameType = t,
        Games = games.Where(g => g.GameTypeId == t.Id).ToList()
    })
    .ToList();

// ═══════════════════════════════════════════════════════════════════════════════
// CONSOLE OUTPUT FOR LINQ QUERIES
// ═══════════════════════════════════════════════════════════════════════════════

Console.WriteLine("\n═ BOARD GAME REVIEWS - LINQ RESULTS ═\n");

// 1. UPCOMING EVENTS
Console.WriteLine($"✓ UPCOMING EVENTS: {upcomingEvents.Count}");
foreach (var evt in upcomingEvents)
    Console.WriteLine($"  • {evt.Name} - {evt.StartDateTime:MMM dd, HH:mm} ({evt.Location})");

// 2. TOP RATED GAMES
Console.WriteLine($"\n✓ TOP RATED GAMES: {topRatedGames.Count}");
foreach (var item in topRatedGames)
    Console.WriteLine($"  • {item.Game.Name} - {item.AverageRating:F1}/5 ({item.Game.YearPublished})");

// 3. POPULAR CATEGORIES
Console.WriteLine($"\n✓ POPULAR CATEGORIES: {popularCategories.Count}");
foreach (var item in popularCategories)
    Console.WriteLine($"  • {item.Category.Name} - {item.Popularity} reviews ({item.Category.AgeGroup})");

// 4. ACTIVE USERS
Console.WriteLine($"\n✓ ACTIVE USERS: {activeUsers.Count}");
foreach (var item in activeUsers)
    Console.WriteLine($"  • {item.User.Username} - {item.ReviewCount} reviews ({item.User.Country})");

// 5. RECOMMENDED GAMES
Console.WriteLine($"\n✓ RECOMMENDED GAMES: {recommendedGames.Count}");
foreach (var game in recommendedGames)
    Console.WriteLine($"  • {game.Name} - {game.Difficulty} ({game.MinPlayers}-{game.MaxPlayers} players)");

Console.WriteLine("\n════════════════════════════════════════════════════════════\n");




builder.Services.AddSingleton(gameTypes);
builder.Services.AddSingleton(publishers);
builder.Services.AddSingleton(categories);
builder.Services.AddSingleton(games);
builder.Services.AddSingleton(users);
builder.Services.AddSingleton(reviews);
builder.Services.AddSingleton(events);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string ResolveContentRootPath()
{
    var currentDirectory = AppContext.BaseDirectory;
    var directory = new DirectoryInfo(currentDirectory);

    while (directory != null)
    {
        var projectFilePath = Path.Combine(directory.FullName, "BoardGameReviews.csproj");
        var webRootPath = Path.Combine(directory.FullName, "wwwroot");

        if (File.Exists(projectFilePath) && Directory.Exists(webRootPath))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return Directory.GetCurrentDirectory();
}
