using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] Difficulty? difficulty,
        [FromQuery] int? minPopularity,
        [FromQuery] int? maxPopularity,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Categories
            .AsNoTracking()
            .Include(c => c.Games)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                (c.Description != null && c.Description.Contains(term)) ||
                (c.AgeGroup != null && c.AgeGroup.Contains(term)));
        }

        if (difficulty.HasValue)
        {
            query = query.Where(c => c.Difficulty == difficulty.Value);
        }

        if (minPopularity.HasValue)
        {
            query = query.Where(c => c.Popularity >= minPopularity.Value);
        }

        if (maxPopularity.HasValue)
        {
            query = query.Where(c => c.Popularity <= maxPopularity.Value);
        }

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
            "popularity" => isDesc ? query.OrderByDescending(c => c.Popularity) : query.OrderBy(c => c.Popularity),
            _ => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                AgeGroup = c.AgeGroup,
                Difficulty = c.Difficulty,
                Popularity = c.Popularity,
                GamesCount = c.Games.Count
            })
            .ToListAsync();

        return Ok(new PagedResultDto<CategoryDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Games)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            AgeGroup = category.AgeGroup,
            Difficulty = category.Difficulty,
            Popularity = category.Popularity,
            GamesCount = category.Games.Count,
            Games = category.Games
                .OrderBy(g => g.Name)
                .Select(g => new RefDto { Id = g.Id, Name = g.Name })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryUpsertDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            AgeGroup = dto.AgeGroup,
            Difficulty = dto.Difficulty,
            Popularity = dto.Popularity
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            AgeGroup = category.AgeGroup,
            Difficulty = category.Difficulty,
            Popularity = category.Popularity,
            GamesCount = 0
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpsertDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.AgeGroup = dto.AgeGroup;
        category.Difficulty = dto.Difficulty;
        category.Popularity = dto.Popularity;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Games)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Games.Count > 0)
        {
            return Conflict(new { message = "Category cannot be deleted while it has related games." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
