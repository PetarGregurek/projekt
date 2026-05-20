using BoardGameReviews.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ResolveContentRootPath()
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBoardGameRepository, EfBoardGameRepository>();

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
    name: "games-list",
    pattern: "games",
    defaults: new { controller = "Game", action = "Index" });

app.MapControllerRoute(
    name: "game-details",
    pattern: "games/{id:int}",
    defaults: new { controller = "Game", action = "Details" });

app.MapControllerRoute(
    name: "categories-list",
    pattern: "categories",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "category-details",
    pattern: "categories/{id:int}",
    defaults: new { controller = "Category", action = "Details" });

app.MapControllerRoute(
    name: "game-types-list",
    pattern: "game-types",
    defaults: new { controller = "GameType", action = "Index" });

app.MapControllerRoute(
    name: "game-type-details",
    pattern: "game-types/{id:int}",
    defaults: new { controller = "GameType", action = "Details" });

app.MapControllerRoute(
    name: "publishers-list",
    pattern: "publishers",
    defaults: new { controller = "Publisher", action = "Index" });

app.MapControllerRoute(
    name: "publisher-details",
    pattern: "publishers/{id:int}",
    defaults: new { controller = "Publisher", action = "Details" });

app.MapControllerRoute(
    name: "users-list",
    pattern: "users",
    defaults: new { controller = "User", action = "Index" });

app.MapControllerRoute(
    name: "user-details",
    pattern: "users/{id:int}",
    defaults: new { controller = "User", action = "Details" });

app.MapControllerRoute(
    name: "events-list",
    pattern: "events",
    defaults: new { controller = "Event", action = "Index" });

app.MapControllerRoute(
    name: "event-details",
    pattern: "events/{id:int}",
    defaults: new { controller = "Event", action = "Details" });

app.MapControllerRoute(
    name: "reviews-list",
    pattern: "reviews",
    defaults: new { controller = "Review", action = "Index" });

app.MapControllerRoute(
    name: "review-details",
    pattern: "reviews/{id:int}",
    defaults: new { controller = "Review", action = "Details" });

app.MapControllerRoute(
    name: "privacy-page",
    pattern: "privacy",
    defaults: new { controller = "Home", action = "Privacy" });

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
