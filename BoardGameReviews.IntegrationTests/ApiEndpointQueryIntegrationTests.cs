using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BoardGameReviews.IntegrationTests.Infrastructure;

namespace BoardGameReviews.IntegrationTests;

public class ApiEndpointQueryIntegrationTests
{
    [Theory]
    [InlineData("/api/categories?q=Strategy&difficulty=2&minPopularity=90&maxPopularity=100&sortBy=popularity&sortDir=desc&page=1&pageSize=2", "Strategy")]
    [InlineData("/api/game-types?q=Card&sortBy=id&sortDir=desc&page=1&pageSize=2", "Card Game")]
    [InlineData("/api/publishers?q=Asmodee&country=France&sortBy=country&sortDir=asc&page=1&pageSize=2", "Asmodee")]
    [InlineData("/api/users?q=StrategyKing&country=Serbia&minAge=30&maxAge=40&sortBy=age&sortDir=desc&page=1&pageSize=2", "StrategyKing")]
    [InlineData("/api/games?q=Catan&categoryId=1&publisherId=1&gameTypeId=1&difficulty=1&minPlayers=2&maxPlayers=4&yearFrom=1990&yearTo=2000&sortBy=yearPublished&sortDir=desc&page=1&pageSize=2", "Catan")]
    [InlineData("/api/events?q=Catan&gameId=1&sortBy=name&sortDir=asc&page=1&pageSize=2", "Catan Tournament")]
    [InlineData("/api/reviews?q=Excellent&gameId=1&userId=1&minRating=5&maxRating=5&recommended=true&sortBy=rating&sortDir=desc&page=1&pageSize=2", "Excellent Classic Game")]
    public async Task Collection_Endpoints_Apply_Search_Filter_Sort_And_Paging(string url, string expectedNameOrTitle)
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(2, root.GetProperty("pageSize").GetInt32());
        Assert.True(root.GetProperty("total").GetInt32() >= 1);

        var firstItem = root.GetProperty("items").EnumerateArray().First();
        var displayValue = firstItem.TryGetProperty("name", out var name)
            ? name.GetString()
            : firstItem.TryGetProperty("username", out var username)
                ? username.GetString()
                : firstItem.GetProperty("title").GetString();

        Assert.Equal(expectedNameOrTitle, displayValue);
    }

    [Theory]
    [InlineData("/api/categories?page=-10&pageSize=500")]
    [InlineData("/api/game-types?page=0&pageSize=0")]
    [InlineData("/api/publishers?page=0&pageSize=101")]
    [InlineData("/api/users?page=-1&pageSize=150")]
    [InlineData("/api/games?page=0&pageSize=1000")]
    [InlineData("/api/events?page=-5&pageSize=250")]
    [InlineData("/api/reviews?page=0&pageSize=999")]
    public async Task Collection_Endpoints_Normalize_Invalid_Paging(string url)
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.InRange(root.GetProperty("pageSize").GetInt32(), 20, 100);
    }

    [Theory]
    [InlineData("/api/categories/1", "games")]
    [InlineData("/api/game-types/1", "games")]
    [InlineData("/api/publishers/1", "games")]
    [InlineData("/api/users/1", "reviews")]
    [InlineData("/api/games/1", "reviews")]
    [InlineData("/api/games/1", "events")]
    public async Task Detail_Endpoints_Return_Related_Collections(string url, string relatedCollectionName)
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.TryGetProperty(relatedCollectionName, out var related));
        Assert.Equal(JsonValueKind.Array, related.ValueKind);
        Assert.NotEmpty(related.EnumerateArray());
    }

    [Theory]
    [InlineData("/api/categories/1")]
    [InlineData("/api/game-types/1")]
    [InlineData("/api/publishers/1")]
    [InlineData("/api/users/1")]
    [InlineData("/api/games/1")]
    public async Task Delete_Endpoints_Return_Conflict_When_Related_Data_Exists(string url)
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(url);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task Review_Create_Defaults_CreatedAt_When_Not_Provided()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/reviews", new
        {
            rating = 4,
            title = "Default Date Review",
            comment = "CreatedAt should be assigned by the API.",
            isRecommended = true,
            gameId = 1,
            userId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("createdAt").GetDateTime() > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Soft_Deleted_Game_Is_Not_Returned_By_Games_Endpoint()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/games", new
        {
            name = "Soft Deleted Integration Game",
            description = "Temporary game without reviews.",
            yearPublished = 2024,
            minPlayers = 1,
            maxPlayers = 4,
            difficulty = 1,
            gameTypeId = 1,
            publisherId = 1,
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var createdJson = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = createdJson.RootElement.GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/games/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/games/{id}")).StatusCode);

        var list = await client.GetAsync("/api/games?q=Soft%20Deleted%20Integration%20Game");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        using var listJson = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(0, listJson.RootElement.GetProperty("total").GetInt32());
    }
}
