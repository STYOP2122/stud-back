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
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        var stats = new AdminStatsDto(
            await db.Users.CountAsync(),
            await db.Orders.CountAsync(),
            await db.Orders.CountAsync(o => o.Status == OrderStatus.Open),
            await db.Users.CountAsync(u => u.IsPro),
            await db.Users.CountAsync(u => u.IsBanned)
        );
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        var users = await db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return Ok(users.Select(u => u.ToAdminDto()).ToList());
    }

    [HttpPut("users/{id}/pro")]
    public async Task<ActionResult<AdminUserDto>> SetPro(int id, SetProRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == UserRole.Admin)
            return BadRequest(new { message = "Нельзя изменить PRO у администратора" });

        user.IsPro = request.IsPro;
        await db.SaveChangesAsync();
        return Ok(user.ToAdminDto());
    }

    [HttpPut("users/{id}/ban")]
    public async Task<ActionResult<AdminUserDto>> SetBan(int id, SetBanRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == UserRole.Admin)
            return BadRequest(new { message = "Нельзя заблокировать администратора" });

        user.IsBanned = request.IsBanned;
        await db.SaveChangesAsync();
        return Ok(user.ToAdminDto());
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        var orders = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .Include(o => o.Bids)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => o.ToDto()).ToList());
    }

    [HttpDelete("orders/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("users/{id}/role")]
    public async Task<ActionResult<AdminUserDto>> SetRole(int id, [FromBody] UserRole role)
    {
        if (role == UserRole.Admin)
            return BadRequest(new { message = "Нельзя назначить роль администратора через API" });

        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == UserRole.Admin)
            return BadRequest(new { message = "Нельзя изменить роль администратора" });

        user.Role = role;
        await db.SaveChangesAsync();
        return Ok(user.ToAdminDto());
    }
}
