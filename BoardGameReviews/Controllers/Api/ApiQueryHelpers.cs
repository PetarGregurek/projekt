namespace BoardGameReviews.Controllers.Api;

internal static class ApiQueryHelpers
{
    public static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize, int maxPageSize = 100)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize switch
        {
            < 1 => 20,
            > 100 => maxPageSize,
            _ => pageSize
        };

        return (safePage, safePageSize, (safePage - 1) * safePageSize);
    }

    public static bool IsDesc(string? sortDir) =>
        string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
