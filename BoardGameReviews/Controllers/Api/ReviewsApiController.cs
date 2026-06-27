using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/reviews")]
public class ReviewsApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReviewsApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int? gameId,
        [FromQuery] int? userId,
        [FromQuery] int? minRating,
        [FromQuery] int? maxRating,
        [FromQuery] bool? recommended,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortDir = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(r =>
                r.Title.Contains(term) ||
                (r.Comment != null && r.Comment.Contains(term)) ||
                (r.Game != null && r.Game.Name.Contains(term)) ||
                (r.User != null && r.User.Username.Contains(term)));
        }

        if (gameId.HasValue) query = query.Where(r => r.GameId == gameId.Value);
        if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);
        if (minRating.HasValue) query = query.Where(r => r.Rating >= minRating.Value);
        if (maxRating.HasValue) query = query.Where(r => r.Rating <= maxRating.Value);
        if (recommended.HasValue) query = query.Where(r => r.IsRecommended == recommended.Value);
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value);

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "createdAt").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
            "rating" => isDesc ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
            _ => isDesc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsRecommended = r.IsRecommended,
                CreatedAt = r.CreatedAt,
                Game = r.Game == null ? null : new RefDto { Id = r.Game.Id, Name = r.Game.Name },
                User = r.User == null ? null : new RefDto { Id = r.User.Id, Name = r.User.Username }
            })
            .ToListAsync();

        return Ok(new PagedResultDto<ReviewDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReviewDto>> GetById(int id)
    {
        var review = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            return NotFound();
        }

        return Ok(new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsRecommended = review.IsRecommended,
            CreatedAt = review.CreatedAt,
            Game = review.Game == null ? null : new RefDto { Id = review.Game.Id, Name = review.Game.Name },
            User = review.User == null ? null : new RefDto { Id = review.User.Id, Name = review.User.Username }
        });
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] ReviewUpsertDto dto)
    {
        if (!await _db.Games.AnyAsync(g => g.Id == dto.GameId) ||
            !await _db.Users.AnyAsync(u => u.Id == dto.UserId))
        {
            return BadRequest(new { message = "Game or user does not exist." });
        }

        var createdAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt;

        var review = new Review
        {
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            IsRecommended = dto.IsRecommended,
            CreatedAt = createdAt,
            GameId = dto.GameId,
            UserId = dto.UserId
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = review.Id }, new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsRecommended = review.IsRecommended,
            CreatedAt = review.CreatedAt
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<IActionResult> Update(int id, [FromBody] ReviewUpsertDto dto)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        if (!await _db.Games.AnyAsync(g => g.Id == dto.GameId) ||
            !await _db.Users.AnyAsync(u => u.Id == dto.UserId))
        {
            return BadRequest(new { message = "Game or user does not exist." });
        }

        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Comment = dto.Comment;
        review.IsRecommended = dto.IsRecommended;
        review.CreatedAt = dto.CreatedAt == default ? review.CreatedAt : dto.CreatedAt;
        review.GameId = dto.GameId;
        review.UserId = dto.UserId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
