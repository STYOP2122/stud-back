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
public class OrdersController(AppDbContext db, FileStorageService fileStorage) : ControllerBase
{
    private (int? userId, UserRole? role) GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return (null, null);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
        return (userId, role);
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] WorkType? workType,
        [FromQuery] string? subject,
        [FromQuery] string? search)
    {
        var (userId, role) = GetCurrentUser();

        var query = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .Include(o => o.Bids)
            .AsQueryable();

        if (role != UserRole.Admin)
        {
            query = query.Where(o =>
                !o.IsPrivate ||
                (userId != null && (
                    o.CustomerId == userId ||
                    o.InvitedExecutorId == userId ||
                    o.ExecutorId == userId
                )));
        }

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (workType.HasValue)
            query = query.Where(o => o.WorkType == workType.Value);

        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(o => o.Subject.Contains(subject));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o =>
                o.Title.Contains(search) || o.Description.Contains(search));

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => o.ToDto()).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
    {
        var (userId, role) = GetCurrentUser();

        var order = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .Include(o => o.Review!)
                .ThenInclude(r => r.FromUser)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (!OrderAccessHelper.CanView(order, userId, role)) return Forbid();

        return Ok(await order.ToDetailDtoAsync(db, fileStorage));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);

        if (user?.Role != UserRole.Customer)
            return Forbid();

        if (request.Deadline <= DateTime.UtcNow)
            return BadRequest(new { message = "Дедлайн должен быть в будущем" });

        if (request.IsPrivate)
        {
            if (!request.InvitedExecutorId.HasValue)
                return BadRequest(new { message = "Укажите исполнителя для приватного заказа" });

            var invited = await db.Users.FindAsync(request.InvitedExecutorId.Value);
            if (invited == null || invited.Role != UserRole.Executor || invited.IsBanned)
                return BadRequest(new { message = "Исполнитель не найден" });
        }

        var order = new Order
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            WorkType = request.WorkType,
            Subject = request.Subject.Trim(),
            Budget = request.Budget,
            Deadline = request.Deadline.ToUniversalTime(),
            CustomerId = userId,
            IsPrivate = request.IsPrivate,
            InvitedExecutorId = request.IsPrivate ? request.InvitedExecutorId : null
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await db.Entry(order).Reference(o => o.Customer).LoadAsync();
        if (order.InvitedExecutorId.HasValue)
            await db.Entry(order).Reference(o => o.InvitedExecutor).LoadAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order.ToDto());
    }

    [Authorize]
    [HttpPost("{id}/accept-bid/{bidId}")]
    public async Task<ActionResult<OrderDetailDto>> AcceptBid(int id, int bidId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.InvitedExecutor)
            .Include(o => o.Bids)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.CustomerId != userId) return Forbid();
        if (order.Status != OrderStatus.Open)
            return BadRequest(new { message = "Заказ уже не открыт" });

        var bid = order.Bids.FirstOrDefault(b => b.Id == bidId);
        if (bid == null) return NotFound();

        foreach (var b in order.Bids)
            b.IsAccepted = b.Id == bidId;

        order.ExecutorId = bid.ExecutorId;
        order.Status = OrderStatus.InProgress;
        order.Budget = bid.Price;

        await db.SaveChangesAsync();

        await db.Entry(order).Reference(o => o.Executor).LoadAsync();
        return Ok(await order.ToDetailDtoAsync(db, fileStorage));
    }

    [Authorize]
    [HttpPost("{id}/complete")]
    public async Task<ActionResult<OrderDetailDto>> CompleteOrder(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.ExecutorId != userId) return Forbid();
        if (order.Status != OrderStatus.InProgress)
            return BadRequest(new { message = "Заказ не в работе" });

        order.Status = OrderStatus.Completed;
        if (order.Executor != null)
            order.Executor.CompletedOrders++;

        await db.SaveChangesAsync();
        return Ok(await order.ToDetailDtoAsync(db, fileStorage));
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDetailDto>> CancelOrder(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.CustomerId != userId) return Forbid();
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            return BadRequest(new { message = "Нельзя отменить этот заказ" });

        order.Status = OrderStatus.Cancelled;
        order.ExecutorId = null;

        await db.SaveChangesAsync();
        return Ok(await order.ToDetailDtoAsync(db, fileStorage));
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<List<OrderDto>>> MyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        IQueryable<Order> query = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Executor)
            .Include(o => o.InvitedExecutor)
            .Include(o => o.Bids);

        query = user.Role switch
        {
            UserRole.Customer => query.Where(o => o.CustomerId == userId),
            UserRole.Executor => query.Where(o =>
                o.ExecutorId == userId ||
                o.Bids.Any(b => b.ExecutorId == userId) ||
                o.InvitedExecutorId == userId),
            UserRole.Admin => query,
            _ => query.Where(o => false)
        };

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(o => o.ToDto()).ToList());
    }
}
