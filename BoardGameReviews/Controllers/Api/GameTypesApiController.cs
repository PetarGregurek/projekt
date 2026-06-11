using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/game-types")]
public class GameTypesApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public GameTypesApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<GameTypeDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.GameTypes
            .AsNoTracking()
            .Include(t => t.Games)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(t =>
                t.Name.Contains(term) ||
                (t.Description != null && t.Description.Contains(term)));
        }

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
            _ => isDesc ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(t => new GameTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                GamesCount = t.Games.Count
            })
            .ToListAsync();

        return Ok(new PagedResultDto<GameTypeDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameTypeDto>> GetById(int id)
    {
        var gameType = await _db.GameTypes
            .AsNoTracking()
            .Include(t => t.Games)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (gameType == null)
        {
            return NotFound();
        }

        return Ok(new GameTypeDto
        {
            Id = gameType.Id,
            Name = gameType.Name,
            Description = gameType.Description,
            GamesCount = gameType.Games.Count,
            Games = gameType.Games
                .OrderBy(g => g.Name)
                .Select(g => new RefDto { Id = g.Id, Name = g.Name })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<GameTypeDto>> Create([FromBody] GameTypeUpsertDto dto)
    {
        var gameType = new GameType
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.GameTypes.Add(gameType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = gameType.Id }, new GameTypeDto
        {
            Id = gameType.Id,
            Name = gameType.Name,
            Description = gameType.Description,
            GamesCount = 0
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GameTypeUpsertDto dto)
    {
        var gameType = await _db.GameTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (gameType == null)
        {
            return NotFound();
        }

        gameType.Name = dto.Name;
        gameType.Description = dto.Description;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gameType = await _db.GameTypes
            .Include(t => t.Games)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (gameType == null)
        {
            return NotFound();
        }

        if (gameType.Games.Count > 0)
        {
            return Conflict(new { message = "Game type cannot be deleted while it has related games." });
        }

        _db.GameTypes.Remove(gameType);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
