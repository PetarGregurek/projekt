using System.Net;
using Microsoft.Playwright;

namespace BoardGameReviews.IntegrationTests;

public class PlaywrightGameCrudScenarioTests
{
    [Fact]
    public async Task Admin_Can_Create_Edit_Search_Delete_And_Logout_Game()
    {
        var baseUrl = GetBaseUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        var adminLogin = Environment.GetEnvironmentVariable("E2E_ADMIN_LOGIN") ?? "admin@boardgamereviews.local";
        var adminPassword = Environment.GetEnvironmentVariable("E2E_ADMIN_PASSWORD") ?? "Admin123!";
        var runId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var gameName = $"Playwright Game {runId}";
        var updatedGameName = $"{gameName} Updated";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !string.Equals(Environment.GetEnvironmentVariable("E2E_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase)
        });

        var page = await browser.NewPageAsync(new BrowserNewPageOptions { IgnoreHTTPSErrors = true });

        // 1. Login
        await page.GotoAsync($"{baseUrl}/Identity/Account/Login");
        await page.GetByLabel("Email or username").FillAsync(adminLogin);
        await page.GetByLabel("Password").FillAsync(adminPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).WaitForAsync();

        // 2. Open list
        await page.GotoAsync($"{baseUrl}/games");
        await page.GetByRole(AriaRole.Heading, new() { Name = "Games" }).WaitForAsync();

        // 3. Create record
        await page.GetByRole(AriaRole.Link, new() { Name = "New Game" }).ClickAsync();
        await FillGameFormAsync(page, gameName, "Created by Playwright scenario.", 2026, 2, 4, "Medium");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create Game" }).ClickAsync();

        // 4. Verify it exists
        await page.GetByRole(AriaRole.Heading, new() { Name = gameName }).WaitForAsync();
        Assert.Contains("Created by Playwright scenario.", await page.Locator("body").TextContentAsync());

        // 5. Edit
        await page.GotoAsync($"{baseUrl}/games");
        await SearchGamesAsync(page, gameName);
        var createdRow = page.Locator("tbody tr").Filter(new() { HasText = gameName }).First;
        await createdRow.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();
        await page.Locator("[name='Input.Name']").FillAsync(updatedGameName);
        await page.Locator("[name='Input.Description']").FillAsync("Updated by Playwright scenario.");
        await page.Locator("[name='Input.MaxPlayers']").FillAsync("5");

        // 6. Save
        await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

        // 7. Verify changes
        await page.GetByRole(AriaRole.Heading, new() { Name = updatedGameName }).WaitForAsync();
        var detailsText = await page.Locator("body").TextContentAsync();
        Assert.Contains("Updated by Playwright scenario.", detailsText);
        Assert.Contains("2 - 5", detailsText);

        // 8. Search
        await page.GotoAsync($"{baseUrl}/games");
        await SearchGamesAsync(page, updatedGameName);
        var updatedRow = page.Locator("tbody tr").Filter(new() { HasText = updatedGameName }).First;
        await updatedRow.WaitForAsync();

        // 9. Delete
        await updatedRow.GetByRole(AriaRole.Link, new() { Name = "Delete" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" }).ClickAsync();
        await SearchGamesAsync(page, updatedGameName);
        Assert.Contains("No results found", await page.Locator("#games-table-body").TextContentAsync());

        // 10. Logout
        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Login" }).WaitForAsync();
    }

    private static async Task FillGameFormAsync(
        IPage page,
        string name,
        string description,
        int year,
        int minPlayers,
        int maxPlayers,
        string difficulty)
    {
        await page.Locator("[name='Input.Name']").FillAsync(name);
        await page.Locator("[name='Input.YearPublished']").FillAsync(year.ToString());
        await page.Locator("[name='Input.Description']").FillAsync(description);
        await page.Locator("[name='Input.MinPlayers']").FillAsync(minPlayers.ToString());
        await page.Locator("[name='Input.MaxPlayers']").FillAsync(maxPlayers.ToString());
        await page.Locator("[name='Input.Difficulty']").SelectOptionAsync(new SelectOptionValue { Label = difficulty });
        await ChooseAutocompleteAsync(page, "Game Type", "Board", "Board Game");
        await ChooseAutocompleteAsync(page, "Publisher", "Catan", "Catan Studio");
        await ChooseAutocompleteAsync(page, "Category", "Strategy", "Strategy");
    }

    private static async Task ChooseAutocompleteAsync(IPage page, string label, string query, string optionText)
    {
        var root = page.Locator(".autocomplete-dropdown").Filter(new() { HasText = label }).First;
        var input = root.Locator("[data-autocomplete-input]");
        await input.FillAsync(query);

        var option = root.Locator("[data-autocomplete-option]").Filter(new() { HasText = optionText }).First;
        await option.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await option.ClickAsync();
    }

    private static async Task SearchGamesAsync(IPage page, string search)
    {
        var searchInput = page.Locator("#game-search");
        await searchInput.FillAsync(search);
        await page.WaitForResponseAsync(response =>
            response.Url.Contains("/Game", StringComparison.OrdinalIgnoreCase) &&
            response.Status == (int)HttpStatusCode.OK);
    }

    private static string? GetBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL")
            ?? Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL");

        return string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl.TrimEnd('/');
    }
}
