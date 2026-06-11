using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/publishers")]
public class PublishersApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public PublishersApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PublisherDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? country,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Publishers
            .AsNoTracking()
            .Include(p => p.Games)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.Country != null && p.Country.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            var countryTerm = country.Trim();
            query = query.Where(p => p.Country != null && p.Country.Contains(countryTerm));
        }

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
            "country" => isDesc ? query.OrderByDescending(p => p.Country) : query.OrderBy(p => p.Country),
            _ => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(p => new PublisherDto
            {
                Id = p.Id,
                Name = p.Name,
                Country = p.Country,
                GamesCount = p.Games.Count
            })
            .ToListAsync();

        return Ok(new PagedResultDto<PublisherDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublisherDto>> GetById(int id)
    {
        var publisher = await _db.Publishers
            .AsNoTracking()
            .Include(p => p.Games)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
        {
            return NotFound();
        }

        return Ok(new PublisherDto
        {
            Id = publisher.Id,
            Name = publisher.Name,
            Country = publisher.Country,
            GamesCount = publisher.Games.Count,
            Games = publisher.Games
                .OrderBy(g => g.Name)
                .Select(g => new RefDto { Id = g.Id, Name = g.Name })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<PublisherDto>> Create([FromBody] PublisherUpsertDto dto)
    {
        var publisher = new Publisher
        {
            Name = dto.Name,
            Country = dto.Country
        };

        _db.Publishers.Add(publisher);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = publisher.Id }, new PublisherDto
        {
            Id = publisher.Id,
            Name = publisher.Name,
            Country = publisher.Country,
            GamesCount = 0
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PublisherUpsertDto dto)
    {
        var publisher = await _db.Publishers.FirstOrDefaultAsync(p => p.Id == id);
        if (publisher == null)
        {
            return NotFound();
        }

        publisher.Name = dto.Name;
        publisher.Country = dto.Country;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var publisher = await _db.Publishers
            .Include(p => p.Games)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
        {
            return NotFound();
        }

        if (publisher.Games.Count > 0)
        {
            return Conflict(new { message = "Publisher cannot be deleted while it has related games." });
        }

        _db.Publishers.Remove(publisher);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
