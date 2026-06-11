using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/games")]
public class GamesApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public GamesApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<GameDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] int? publisherId,
        [FromQuery] int? gameTypeId,
        [FromQuery] Difficulty? difficulty,
        [FromQuery] int? minPlayers,
        [FromQuery] int? maxPlayers,
        [FromQuery] int? yearFrom,
        [FromQuery] int? yearTo,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Games
            .AsNoTracking()
            .Include(g => g.Category)
            .Include(g => g.Publisher)
            .Include(g => g.GameType)
            .Include(g => g.Reviews)
            .Include(g => g.Events)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(g =>
                g.Name.Contains(term) ||
                (g.Description != null && g.Description.Contains(term)) ||
                (g.Category != null && g.Category.Name.Contains(term)) ||
                (g.Publisher != null && g.Publisher.Name.Contains(term)) ||
                (g.GameType != null && g.GameType.Name.Contains(term)));
        }

        if (categoryId.HasValue) query = query.Where(g => g.CategoryId == categoryId.Value);
        if (publisherId.HasValue) query = query.Where(g => g.PublisherId == publisherId.Value);
        if (gameTypeId.HasValue) query = query.Where(g => g.GameTypeId == gameTypeId.Value);
        if (difficulty.HasValue) query = query.Where(g => g.Difficulty == difficulty.Value);
        if (minPlayers.HasValue) query = query.Where(g => g.MinPlayers >= minPlayers.Value);
        if (maxPlayers.HasValue) query = query.Where(g => g.MaxPlayers <= maxPlayers.Value);
        if (yearFrom.HasValue) query = query.Where(g => g.YearPublished >= yearFrom.Value);
        if (yearTo.HasValue) query = query.Where(g => g.YearPublished <= yearTo.Value);

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(g => g.Id) : query.OrderBy(g => g.Id),
            "yearpublished" => isDesc ? query.OrderByDescending(g => g.YearPublished) : query.OrderBy(g => g.YearPublished),
            _ => isDesc ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                YearPublished = g.YearPublished,
                MinPlayers = g.MinPlayers,
                MaxPlayers = g.MaxPlayers,
                Difficulty = g.Difficulty,
                Category = g.Category == null ? null : new RefDto { Id = g.Category.Id, Name = g.Category.Name },
                Publisher = g.Publisher == null ? null : new RefDto { Id = g.Publisher.Id, Name = g.Publisher.Name },
                GameType = g.GameType == null ? null : new RefDto { Id = g.GameType.Id, Name = g.GameType.Name },
                ReviewsCount = g.Reviews.Count,
                EventsCount = g.Events.Count
            })
            .ToListAsync();

        return Ok(new PagedResultDto<GameDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDto>> GetById(int id)
    {
        var game = await _db.Games
            .AsNoTracking()
            .Include(g => g.Category)
            .Include(g => g.Publisher)
            .Include(g => g.GameType)
            .Include(g => g.Reviews)
            .ThenInclude(r => r.User)
            .Include(g => g.Events)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
        {
            return NotFound();
        }

        return Ok(new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Description = game.Description,
            YearPublished = game.YearPublished,
            MinPlayers = game.MinPlayers,
            MaxPlayers = game.MaxPlayers,
            Difficulty = game.Difficulty,
            Category = game.Category == null ? null : new RefDto { Id = game.Category.Id, Name = game.Category.Name },
            Publisher = game.Publisher == null ? null : new RefDto { Id = game.Publisher.Id, Name = game.Publisher.Name },
            GameType = game.GameType == null ? null : new RefDto { Id = game.GameType.Id, Name = game.GameType.Name },
            ReviewsCount = game.Reviews.Count,
            EventsCount = game.Events.Count,
            Reviews = game.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    IsRecommended = r.IsRecommended,
                    CreatedAt = r.CreatedAt,
                    Game = new RefDto { Id = game.Id, Name = game.Name },
                    User = r.User == null ? null : new RefDto { Id = r.User.Id, Name = r.User.Username }
                })
                .ToList(),
            Events = game.Events
                .OrderBy(e => e.StartDateTime)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    Location = e.Location,
                    Game = new RefDto { Id = game.Id, Name = game.Name }
                })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] GameUpsertDto dto)
    {
        if (dto.MaxPlayers < dto.MinPlayers)
        {
            return BadRequest(new { message = "MaxPlayers must be greater than or equal to MinPlayers." });
        }

        if (!await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId) ||
            !await _db.GameTypes.AnyAsync(t => t.Id == dto.GameTypeId) ||
            !await _db.Publishers.AnyAsync(p => p.Id == dto.PublisherId))
        {
            return BadRequest(new { message = "One or more related entities do not exist." });
        }

        var game = new Game
        {
            Name = dto.Name,
            Description = dto.Description,
            YearPublished = dto.YearPublished,
            MinPlayers = dto.MinPlayers,
            MaxPlayers = dto.MaxPlayers,
            Difficulty = dto.Difficulty,
            CategoryId = dto.CategoryId,
            GameTypeId = dto.GameTypeId,
            PublisherId = dto.PublisherId
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = game.Id }, new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Description = game.Description,
            YearPublished = game.YearPublished,
            MinPlayers = game.MinPlayers,
            MaxPlayers = game.MaxPlayers,
            Difficulty = game.Difficulty,
            ReviewsCount = 0,
            EventsCount = 0
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GameUpsertDto dto)
    {
        if (dto.MaxPlayers < dto.MinPlayers)
        {
            return BadRequest(new { message = "MaxPlayers must be greater than or equal to MinPlayers." });
        }

        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game == null)
        {
            return NotFound();
        }

        if (!await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId) ||
            !await _db.GameTypes.AnyAsync(t => t.Id == dto.GameTypeId) ||
            !await _db.Publishers.AnyAsync(p => p.Id == dto.PublisherId))
        {
            return BadRequest(new { message = "One or more related entities do not exist." });
        }

        game.Name = dto.Name;
        game.Description = dto.Description;
        game.YearPublished = dto.YearPublished;
        game.MinPlayers = dto.MinPlayers;
        game.MaxPlayers = dto.MaxPlayers;
        game.Difficulty = dto.Difficulty;
        game.CategoryId = dto.CategoryId;
        game.GameTypeId = dto.GameTypeId;
        game.PublisherId = dto.PublisherId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _db.Games
            .Include(g => g.Reviews)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
        {
            return NotFound();
        }

        if (game.Reviews.Count > 0)
        {
            return Conflict(new { message = "Game cannot be deleted while it has related reviews." });
        }

        game.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
