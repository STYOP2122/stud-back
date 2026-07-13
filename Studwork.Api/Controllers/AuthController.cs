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
public class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
            return BadRequest(new { message = "Нельзя зарегистрироваться как администратор" });

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email уже зарегистрирован" });

        if (request.Password.Length < 6)
            return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name.Trim(),
            Role = request.Role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.ToDto()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });

        if (user.IsBanned)
            return Unauthorized(new { message = "Аккаунт заблокирован" });

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.ToDto()));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();
        return Ok(user.ToDto());
    }
}
