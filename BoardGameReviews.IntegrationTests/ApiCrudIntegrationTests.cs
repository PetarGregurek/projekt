using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BoardGameReviews.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BoardGameReviews.IntegrationTests;

public class CategoriesApiIntegrationTests
{
    [Fact]
    public async Task Categories_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var getAll = await client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, getAll.StatusCode);

        var getById = await client.GetAsync("/api/categories/1");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);

        var getMissing = await client.GetAsync("/api/categories/999999");
        Assert.Equal(HttpStatusCode.NotFound, getMissing.StatusCode);

        var createPayload = new
        {
            name = "Integration Category",
            description = "Created by integration test",
            ageGroup = "12+",
            difficulty = 1,
            popularity = 50
        };

        var create = await client.PostAsJsonAsync("/api/categories", createPayload);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var updatePayload = new
        {
            name = "Integration Category Updated",
            description = "Updated",
            ageGroup = "13+",
            difficulty = 2,
            popularity = 60
        };

        var update = await client.PutAsJsonAsync($"/api/categories/{created!.Id}", updatePayload);
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var updateMissing = await client.PutAsJsonAsync("/api/categories/999999", updatePayload);
        Assert.Equal(HttpStatusCode.NotFound, updateMissing.StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/categories", new { description = "missing name" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var delete = await client.DeleteAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var deleteMissing = await client.DeleteAsync("/api/categories/999999");
        Assert.Equal(HttpStatusCode.NotFound, deleteMissing.StatusCode);
    }
}

public class GameTypesApiIntegrationTests
{
    [Fact]
    public async Task GameTypes_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/game-types")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/game-types/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/game-types/999999")).StatusCode);

        var create = await client.PostAsJsonAsync("/api/game-types", new { name = "Integration Type", description = "Desc" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/game-types/{created!.Id}", new { name = "Integration Type 2", description = "Updated" });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/game-types/999999", new { name = "X", description = "Y" })).StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/game-types", new { description = "no name" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/game-types/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/game-types/999999")).StatusCode);
    }
}

public class PublishersApiIntegrationTests
{
    [Fact]
    public async Task Publishers_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/publishers")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/publishers/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/publishers/999999")).StatusCode);

        var create = await client.PostAsJsonAsync("/api/publishers", new { name = "Integration Publisher", country = "HR" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync($"/api/publishers/{created!.Id}", new { name = "Integration Publisher 2", country = "DE" })).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/publishers/999999", new { name = "X", country = "Y" })).StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/publishers", new { country = "missing name" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/publishers/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/publishers/999999")).StatusCode);
    }
}

public class UsersApiIntegrationTests
{
    [Fact]
    public async Task Users_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/users/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/users/999999")).StatusCode);

        var create = await client.PostAsJsonAsync("/api/users", new
        {
            username = "integration-user",
            email = "integration-user@example.test",
            password = "hashed",
            country = "HR",
            age = 25
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/users/{created!.Id}", new
        {
            username = "integration-user-2",
            email = "integration-user-2@example.test",
            password = "hashed2",
            country = "DE",
            age = 30
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/users/999999", new
            {
                username = "x",
                email = "x@example.test",
                password = "x",
                country = "x",
                age = 22
            })).StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/users", new { username = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/users/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/users/999999")).StatusCode);
    }
}

public class GamesApiIntegrationTests
{
    [Fact]
    public async Task Games_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/games/999999")).StatusCode);

        var createPayload = new
        {
            name = "Integration Game",
            description = "Desc",
            yearPublished = 2020,
            minPlayers = 2,
            maxPlayers = 4,
            difficulty = 1,
            gameTypeId = 1,
            publisherId = 1,
            categoryId = 1
        };

        var create = await client.PostAsJsonAsync("/api/games", createPayload);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/games/{created!.Id}", new
        {
            name = "Integration Game Updated",
            description = "Updated",
            yearPublished = 2021,
            minPlayers = 2,
            maxPlayers = 5,
            difficulty = 2,
            gameTypeId = 1,
            publisherId = 1,
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/games/999999", createPayload)).StatusCode);

        // Validation: MaxPlayers < MinPlayers
        var invalid = await client.PostAsJsonAsync("/api/games", new
        {
            name = "Invalid Game",
            description = "Invalid",
            yearPublished = 2020,
            minPlayers = 5,
            maxPlayers = 2,
            difficulty = 1,
            gameTypeId = 1,
            publisherId = 1,
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/games/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/games/999999")).StatusCode);
    }
}

public class EventsApiIntegrationTests
{
    [Fact]
    public async Task Events_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/events")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/events/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/events/999999")).StatusCode);

        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(3);

        var createPayload = new
        {
            name = "Integration Event",
            gameId = 1,
            startDateTime = start,
            endDateTime = end,
            location = "Test Venue"
        };

        var create = await client.PostAsJsonAsync("/api/events", createPayload);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/events/{created!.Id}", new
        {
            name = "Integration Event Updated",
            gameId = 1,
            startDateTime = start,
            endDateTime = end.AddHours(1),
            location = "New Venue"
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/events/999999", createPayload)).StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/events", new
        {
            name = "Invalid Event",
            gameId = 1,
            startDateTime = end,
            endDateTime = start,
            location = "Bad"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/events/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/events/999999")).StatusCode);
    }
}

public class ReviewsApiIntegrationTests
{
    [Fact]
    public async Task Reviews_CRUD_And_Validation_Work()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/reviews")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/reviews/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/reviews/999999")).StatusCode);

        var createPayload = new
        {
            rating = 4,
            title = "Integration Review",
            comment = "Looks good",
            isRecommended = true,
            createdAt = DateTime.UtcNow,
            gameId = 1,
            userId = 1
        };

        var create = await client.PostAsJsonAsync("/api/reviews", createPayload);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(created);

        var update = await client.PutAsJsonAsync($"/api/reviews/{created!.Id}", new
        {
            rating = 5,
            title = "Integration Review Updated",
            comment = "Updated",
            isRecommended = true,
            createdAt = DateTime.UtcNow,
            gameId = 1,
            userId = 1
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync("/api/reviews/999999", createPayload)).StatusCode);

        var invalid = await client.PostAsJsonAsync("/api/reviews", new
        {
            rating = 5,
            title = "Invalid",
            comment = "Missing refs",
            isRecommended = true,
            createdAt = DateTime.UtcNow,
            gameId = 999999,
            userId = 999999
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/reviews/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/reviews/999999")).StatusCode);
    }
}

internal sealed class IdResponse
{
    public int Id { get; set; }
}
