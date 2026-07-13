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
public class ReviewsController(AppDbContext db) : ControllerBase
{
    [HttpPost("order/{orderId}")]
    public async Task<ActionResult<ReviewDto>> CreateReview(int orderId, CreateReviewRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders
            .Include(o => o.Review)
            .Include(o => o.Executor)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Completed)
            return BadRequest(new { message = "Отзыв можно оставить только на завершённый заказ" });
        if (order.CustomerId != userId)
            return Forbid();
        if (order.Review != null)
            return BadRequest(new { message = "Отзыв уже оставлен" });
        if (order.ExecutorId == null)
            return BadRequest(new { message = "У заказа нет исполнителя" });
        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new { message = "Рейтинг от 1 до 5" });

        var review = new Review
        {
            OrderId = orderId,
            FromUserId = userId,
            ToUserId = order.ExecutorId.Value,
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        db.Reviews.Add(review);

        var executor = order.Executor!;
        var reviews = await db.Reviews.Where(r => r.ToUserId == executor.Id).ToListAsync();
        reviews.Add(review);
        executor.Rating = reviews.Average(r => r.Rating);

        await db.SaveChangesAsync();

        await db.Entry(review).Reference(r => r.FromUser).LoadAsync();
        return Ok(review.ToDto());
    }
}
