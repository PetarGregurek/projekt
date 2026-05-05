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
