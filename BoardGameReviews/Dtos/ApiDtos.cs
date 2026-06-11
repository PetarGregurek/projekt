using System.ComponentModel.DataAnnotations;
using BoardGameReviews.Models;

namespace BoardGameReviews.Dtos;

public class PagedResultDto<T>
{
    public required IReadOnlyList<T> Items { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class RefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AgeGroup { get; set; }
    public Difficulty Difficulty { get; set; }
    public int Popularity { get; set; }
    public int GamesCount { get; set; }
    public IReadOnlyList<RefDto>? Games { get; set; }
}

public class CategoryUpsertDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? AgeGroup { get; set; }

    public Difficulty Difficulty { get; set; }

    [Range(0, 100)]
    public int Popularity { get; set; }
}

public class GameTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int GamesCount { get; set; }
    public IReadOnlyList<RefDto>? Games { get; set; }
}

public class GameTypeUpsertDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class PublisherDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public int GamesCount { get; set; }
    public IReadOnlyList<RefDto>? Games { get; set; }
}

public class PublisherUpsertDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Country { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Country { get; set; }
    public int Age { get; set; }
    public int ReviewsCount { get; set; }
    public IReadOnlyList<ReviewDto>? Reviews { get; set; }
}

public class UserUpsertDto
{
    [Required]
    [StringLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Country { get; set; }

    [Range(5, 120)]
    public int Age { get; set; }
}

public class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int YearPublished { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public Difficulty Difficulty { get; set; }
    public RefDto? GameType { get; set; }
    public RefDto? Publisher { get; set; }
    public RefDto? Category { get; set; }
    public int ReviewsCount { get; set; }
    public int EventsCount { get; set; }
    public IReadOnlyList<ReviewDto>? Reviews { get; set; }
    public IReadOnlyList<EventDto>? Events { get; set; }
}

public class GameUpsertDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1900, 2200)]
    public int YearPublished { get; set; }

    [Range(1, 100)]
    public int MinPlayers { get; set; }

    [Range(1, 100)]
    public int MaxPlayers { get; set; }

    public Difficulty Difficulty { get; set; }

    public int GameTypeId { get; set; }
    public int PublisherId { get; set; }
    public int CategoryId { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool IsRecommended { get; set; }
    public DateTime CreatedAt { get; set; }
    public RefDto? Game { get; set; }
    public RefDto? User { get; set; }
}

public class ReviewUpsertDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Comment { get; set; }

    public bool IsRecommended { get; set; }

    public DateTime CreatedAt { get; set; }

    public int GameId { get; set; }
    public int UserId { get; set; }
}

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public RefDto? Game { get; set; }
}

public class EventUpsertDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public int GameId { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
}
