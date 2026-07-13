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
public class BidsController(AppDbContext db) : ControllerBase
{
    [HttpPost("order/{orderId}")]
    public async Task<ActionResult<BidDto>> CreateBid(int orderId, CreateBidRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        if (!OrderAccessHelper.CanBid(order, userId, user.Role))
        {
            if (order.IsPrivate)
                return BadRequest(new { message = "Это приватный заказ — откликаться может только приглашённый исполнитель" });
            return BadRequest(new { message = "Нельзя откликнуться на этот заказ" });
        }

        if (await db.Bids.AnyAsync(b => b.OrderId == orderId && b.ExecutorId == userId))
            return BadRequest(new { message = "Вы уже откликнулись на этот заказ" });

        var bid = new Bid
        {
            OrderId = orderId,
            ExecutorId = userId,
            Price = request.Price,
            Message = request.Message.Trim(),
            DaysToComplete = request.DaysToComplete
        };

        db.Bids.Add(bid);
        await db.SaveChangesAsync();

        await db.Entry(bid).Reference(b => b.Executor).LoadAsync();
        return Ok(bid.ToDto());
    }
}
