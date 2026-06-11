using BoardGameReviews.Data;
using BoardGameReviews.Dtos;
using BoardGameReviews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGameReviews.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<UserDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? country,
        [FromQuery] int? minAge,
        [FromQuery] int? maxAge,
        [FromQuery] string? sortBy = "username",
        [FromQuery] string? sortDir = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                u.Username.Contains(term) ||
                u.Email.Contains(term) ||
                (u.Country != null && u.Country.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            var countryTerm = country.Trim();
            query = query.Where(u => u.Country != null && u.Country.Contains(countryTerm));
        }

        if (minAge.HasValue)
        {
            query = query.Where(u => u.Age >= minAge.Value);
        }

        if (maxAge.HasValue)
        {
            query = query.Where(u => u.Age <= maxAge.Value);
        }

        var isDesc = ApiQueryHelpers.IsDesc(sortDir);
        query = (sortBy ?? "username").ToLowerInvariant() switch
        {
            "id" => isDesc ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id),
            "age" => isDesc ? query.OrderByDescending(u => u.Age) : query.OrderBy(u => u.Age),
            _ => isDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username)
        };

        var total = await query.CountAsync();
        var paging = ApiQueryHelpers.NormalizePaging(page, pageSize);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Country = u.Country,
                Age = u.Age,
                ReviewsCount = u.Reviews.Count
            })
            .ToListAsync();

        return Ok(new PagedResultDto<UserDto>
        {
            Items = items,
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Reviews)
            .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Country = user.Country,
            Age = user.Age,
            ReviewsCount = user.Reviews.Count,
            Reviews = user.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    IsRecommended = r.IsRecommended,
                    CreatedAt = r.CreatedAt,
                    Game = r.Game == null ? null : new RefDto { Id = r.Game.Id, Name = r.Game.Name },
                    User = new RefDto { Id = user.Id, Name = user.Username }
                })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserUpsertDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            HashedPassword = dto.Password,
            Country = dto.Country,
            Age = dto.Age
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Country = user.Country,
            Age = user.Age,
            ReviewsCount = 0
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpsertDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.HashedPassword = dto.Password;
        user.Country = dto.Country;
        user.Age = dto.Age;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        if (user.Reviews.Count > 0)
        {
            return Conflict(new { message = "User cannot be deleted while it has related reviews." });
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
