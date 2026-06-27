namespace BoardGameReviews.Models;

public class GlobalSearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<GlobalSearchGroupViewModel> Groups { get; set; } = new();

    public int TotalResults => Groups.Sum(group => group.Results.Count);
    public bool HasQuery => !string.IsNullOrWhiteSpace(Query);
}

public class GlobalSearchGroupViewModel
{
    public string Name { get; set; } = string.Empty;
    public List<GlobalSearchResultViewModel> Results { get; set; } = new();
}

public class GlobalSearchResultViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
