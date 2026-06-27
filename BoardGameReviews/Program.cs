using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ResolveContentRootPath()
});

var logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "log-.txt");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        logsPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

builder.Services
    .AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.CallbackPath = "/signin-google";
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.SaveTokens = true;
            options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.CorrelationCookie.HttpOnly = true;
            options.CorrelationCookie.IsEssential = true;
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                var message = Uri.EscapeDataString(context.Failure?.Message ?? "External authentication failed.");
                context.Response.Redirect($"/Identity/Account/Login?oauthError={message}");
                return Task.CompletedTask;
            };
        });
}

builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnSigningOut = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Authentication");
        var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = context.HttpContext.User.Identity?.Name;

        logger.LogInformation("User logged out. UserId: {UserId}, UserName: {UserName}", userId, userName);
        return Task.CompletedTask;
    };
});

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

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

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
    name: "global-search",
    pattern: "search",
    defaults: new { controller = "Search", action = "Index" });

app.MapControllerRoute(
    name: "privacy-page",
    pattern: "privacy",
    defaults: new { controller = "Home", action = "Privacy" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var appLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
appLogger.LogInformation(
    "Application started. Environment: {Environment}, ContentRootPath: {ContentRootPath}, LogsPath: {LogsPath}",
    app.Environment.EnvironmentName,
    app.Environment.ContentRootPath,
    logsPath);

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }

        await IdentitySeed.SeedAsync(services, app.Configuration);
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    appLogger.LogError(ex, "Application stopped unexpectedly.");
    throw;
}
finally
{
    appLogger.LogInformation("Application stopped.");
    Log.CloseAndFlush();
}

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
