using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/events")]
public class EventsApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<EventDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? gameId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sortBy = "startDateTime",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Events
            .AsNoTracking()
            .Include(e => e.Game)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(e =>
                e.Name.Contains(term) ||
                e.Location.Contains(term) ||
                (e.Game != null && e.Game.Name.Contains(term)));
        }

        if (gameId.HasValue) query = query.Where(e => e.GameId == gameId.Value);
        if (from.HasValue) query = query.Where(e => e.StartDateTime >= from.Value);
        if (to.HasValue) query = query.Where(e => e.EndDateTime <= to.Value);

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "startDateTime").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
            "name" => isDesc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            _ => isDesc ? query.OrderByDescending(e => e.StartDateTime) : query.OrderBy(e => e.StartDateTime)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Location = e.Location,
                Game = e.Game == null ? null : new RefDto { Id = e.Game.Id, Name = e.Game.Name }
            })
            .ToListAsync();

        return Ok(new PagedResultDto<EventDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var evt = await _db.Events
            .AsNoTracking()
            .Include(e => e.Game)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evt == null)
        {
            return NotFound();
        }

        return Ok(new EventDto
        {
            Id = evt.Id,
            Name = evt.Name,
            StartDateTime = evt.StartDateTime,
            EndDateTime = evt.EndDateTime,
            Location = evt.Location,
            Game = evt.Game == null ? null : new RefDto { Id = evt.Game.Id, Name = evt.Game.Name }
        });
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventUpsertDto dto)
    {
        if (dto.EndDateTime <= dto.StartDateTime)
        {
            return BadRequest(new { message = "EndDateTime must be later than StartDateTime." });
        }

        if (!await _db.Games.AnyAsync(g => g.Id == dto.GameId))
        {
            return BadRequest(new { message = "Related game does not exist." });
        }

        var evt = new Event
        {
            Name = dto.Name,
            GameId = dto.GameId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Location = dto.Location
        };

        _db.Events.Add(evt);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = evt.Id }, new EventDto
        {
            Id = evt.Id,
            Name = evt.Name,
            StartDateTime = evt.StartDateTime,
            EndDateTime = evt.EndDateTime,
            Location = evt.Location
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpsertDto dto)
    {
        if (dto.EndDateTime <= dto.StartDateTime)
        {
            return BadRequest(new { message = "EndDateTime must be later than StartDateTime." });
        }

        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null)
        {
            return NotFound();
        }

        if (!await _db.Games.AnyAsync(g => g.Id == dto.GameId))
        {
            return BadRequest(new { message = "Related game does not exist." });
        }

        evt.Name = dto.Name;
        evt.GameId = dto.GameId;
        evt.StartDateTime = dto.StartDateTime;
        evt.EndDateTime = dto.EndDateTime;
        evt.Location = dto.Location;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null)
        {
            return NotFound();
        }

        _db.Events.Remove(evt);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
