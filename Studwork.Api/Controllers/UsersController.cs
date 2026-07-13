using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studwork.Api.Data;
using Studwork.Api.DTOs;
using Studwork.Api.Entities;
using Studwork.Api.Services;

namespace Studwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(AppDbContext db, FileStorageService fileStorage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string? search, [FromQuery] UserRole? role)
    {
        var query = db.Users.Where(u => !u.IsBanned && u.Role != UserRole.Admin).AsQueryable();

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var users = await query.OrderBy(u => u.Name).Take(20).ToListAsync();
        return Ok(users.Select(u => u.ToDto()).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null || user.IsBanned) return NotFound();
        return Ok(user.ToDto());
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Name = request.Name.Trim();
        user.Bio = request.Bio?.Trim();
        user.Phone = request.Phone?.Trim();
        user.Skills = request.Skills?.Trim();
        user.PortfolioUrl = request.PortfolioUrl?.Trim();
        user.City = request.City?.Trim();
        await db.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile file)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        try
        {
            var (storedName, contentType, _) = await fileStorage.SaveAsync(file);
            if (!fileStorage.IsImage(contentType))
                return BadRequest(new { message = "Аватар должен быть изображением" });

            user.AvatarUrl = fileStorage.GetUrl(storedName);
            await db.SaveChangesAsync();
            return Ok(user.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
