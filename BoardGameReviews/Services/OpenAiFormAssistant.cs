using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BoardGameReviews.Data;
using BoardGameReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Services
{
    public class OpenAiFormAssistant : IAiFormAssistant
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;
        private readonly ILogger<OpenAiFormAssistant> _logger;

        public OpenAiFormAssistant(
            HttpClient httpClient,
            IConfiguration configuration,
            AppDbContext db,
            ILogger<OpenAiFormAssistant> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _db = db;
            _logger = logger;
        }

        public async Task<AiFormSuggestion> GenerateSuggestionAsync(AiFormRequest request, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["AI:ApiKey"] ?? _configuration["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
                    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("AI API key is not configured. Set Groq:ApiKey, AI:ApiKey, or GROQ_API_KEY.");
            }

            var entityDefinition = await BuildEntityDefinitionAsync(request.EntityType, cancellationToken);
            if (entityDefinition == null)
            {
                throw new ArgumentException("Unsupported entity type.", nameof(request));
            }

            var endpoint = _configuration["AI:Endpoint"] ?? "https://api.groq.com/openai/v1/responses";
            var model = _configuration["AI:Model"] ?? "llama-3.3-70b-versatile";
            var payload = new
            {
                model,
                input = BuildPrompt(entityDefinition, request.Prompt)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI request failed with status {StatusCode}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException("AI service did not return a valid response.");
            }

            var text = ExtractResponseText(responseBody);
            var suggestion = ParseSuggestion(text);
            NormalizeSuggestion(suggestion, entityDefinition);
            return suggestion;
        }

        private async Task<EntityDefinition?> BuildEntityDefinitionAsync(string entityType, CancellationToken cancellationToken)
        {
            var normalized = entityType.Trim().ToLowerInvariant();
            var difficulties = string.Join(", ", Enum.GetNames<Difficulty>());

            return normalized switch
            {
                "game" => new EntityDefinition(
                    "game",
                    new[]
                    {
                        "Input.Name: required text, max 150",
                        "Input.Description: optional text, max 1000",
                        "Input.YearPublished: integer 1900-2200",
                        "Input.MinPlayers: integer 1-100",
                        "Input.MaxPlayers: integer 1-100 and greater than or equal to MinPlayers",
                        $"Input.Difficulty: one of {difficulties}",
                        "Input.GameTypeId: choose id from available game types",
                        "Input.PublisherId: choose id from available publishers",
                        "Input.CategoryId: choose id from available categories"
                    },
                    await BuildGameLookupsAsync(cancellationToken)),
                "category" => new EntityDefinition(
                    "category",
                    new[]
                    {
                        "Input.Name: required text, max 120",
                        "Input.Description: optional text, max 500",
                        "Input.AgeGroup: optional value like 10+ or 12",
                        $"Input.Difficulty: one of {difficulties}",
                        "Input.Popularity: integer 0-100"
                    },
                    string.Empty),
                "gametype" => new EntityDefinition(
                    "game type",
                    new[]
                    {
                        "Input.Name: required text, max 120",
                        "Input.Description: optional text, max 500"
                    },
                    string.Empty),
                "publisher" => new EntityDefinition(
                    "publisher",
                    new[]
                    {
                        "Input.Name: required text, max 120",
                        "Input.Country: optional country name from the application's country list when possible"
                    },
                    string.Empty),
                "user" => new EntityDefinition(
                    "user",
                    new[]
                    {
                        "Input.Username: required text, max 80",
                        "Input.Email: required valid email, max 120",
                        "Input.Password: optional generated password, max 255",
                        "Input.Country: optional country name from the application's country list when possible",
                        "Input.Age: integer 5-120"
                    },
                    string.Empty),
                "review" => new EntityDefinition(
                    "review",
                    new[]
                    {
                        "Input.Title: required text, max 200",
                        "Input.Rating: integer 1-5",
                        "Input.CreatedAt: optional date and time in yyyy-MM-ddTHH:mm format",
                        "Input.IsRecommended: true or false",
                        "Input.Comment: optional text, max 2000",
                        "Input.GameId: choose id from available games",
                        "Input.UserId: choose id from available users"
                    },
                    await BuildReviewLookupsAsync(cancellationToken)),
                "event" => new EntityDefinition(
                    "event",
                    new[]
                    {
                        "Input.Name: required text, max 150",
                        "Input.Location: required text, max 200",
                        "Input.StartDateTime: required date and time in yyyy-MM-ddTHH:mm format",
                        "Input.EndDateTime: required date and time in yyyy-MM-ddTHH:mm format after StartDateTime",
                        "Input.GameId: choose id from available games"
                    },
                    await BuildEventLookupsAsync(cancellationToken)),
                _ => null
            };
        }

        private async Task<string> BuildGameLookupsAsync(CancellationToken cancellationToken)
        {
            var gameTypes = await _db.GameTypes.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);
            var publishers = await _db.Publishers.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);
            var categories = await _db.Categories.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);

            return $"""
Available game types: {JsonSerializer.Serialize(gameTypes, JsonOptions)}
Available publishers: {JsonSerializer.Serialize(publishers, JsonOptions)}
Available categories: {JsonSerializer.Serialize(categories, JsonOptions)}
""";
        }

        private async Task<string> BuildReviewLookupsAsync(CancellationToken cancellationToken)
        {
            var games = await _db.Games.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);
            var users = await _db.Users.OrderBy(x => x.Username).Select(x => new { x.Id, x.Username }).ToListAsync(cancellationToken);

            return $"""
Available games: {JsonSerializer.Serialize(games, JsonOptions)}
Available users: {JsonSerializer.Serialize(users, JsonOptions)}
""";
        }

        private async Task<string> BuildEventLookupsAsync(CancellationToken cancellationToken)
        {
            var games = await _db.Games.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);
            return $"Available games: {JsonSerializer.Serialize(games, JsonOptions)}";
        }

        private static string BuildPrompt(EntityDefinition entityDefinition, string userPrompt)
        {
            var fields = string.Join(Environment.NewLine, entityDefinition.Fields.Select(f => $"- {f}"));

            return $$"""
You help fill an ASP.NET MVC form for the BoardGameReviews application.
Create one {{entityDefinition.Name}} from the user's request.

Allowed fields:
{{fields}}

{{entityDefinition.LookupContext}}

Return only valid JSON with this exact shape:
{
  "fields": [
    { "field": "Input.Name", "value": "example", "displayValue": null }
  ],
  "notes": "short optional note"
}

Rules:
- Use only allowed field names.
- For select/autocomplete id fields, return the numeric id as a string in value and the chosen name in displayValue.
- If an exact related record is not available, leave that id field out and explain it in notes.
- Do not invent database ids.
- Dates must be local values in yyyy-MM-ddTHH:mm format.
- Keep text within the stated maximum lengths.

User request:
{{userPrompt}}
""";
        }

        private static string ExtractResponseText(string responseBody)
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
            {
                return outputText.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var contentItem in content.EnumerateArray())
                    {
                        if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                        {
                            return text.GetString() ?? string.Empty;
                        }
                    }
                }
            }

            return responseBody;
        }

        private static AiFormSuggestion ParseSuggestion(string text)
        {
            var trimmed = text.Trim();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                var firstNewLine = trimmed.IndexOf('\n');
                var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    trimmed = trimmed[(firstNewLine + 1)..lastFence].Trim();
                }
            }

            var suggestion = JsonSerializer.Deserialize<AiFormSuggestion>(trimmed, JsonOptions);
            if (suggestion == null)
            {
                throw new InvalidOperationException("AI response could not be parsed.");
            }

            return suggestion;
        }

        private static void NormalizeSuggestion(AiFormSuggestion suggestion, EntityDefinition entityDefinition)
        {
            var allowedFields = entityDefinition.Fields
                .Select(f => f.Split(':', 2)[0].Trim())
                .ToHashSet(StringComparer.Ordinal);

            suggestion.Fields = suggestion.Fields
                .Where(f => allowedFields.Contains(f.Field))
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .GroupBy(f => f.Field)
                .Select(g => g.First())
                .ToList();
        }

        private sealed record EntityDefinition(string Name, IReadOnlyCollection<string> Fields, string LookupContext);
    }
}
