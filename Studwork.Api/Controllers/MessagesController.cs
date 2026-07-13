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
public class MessagesController(AppDbContext db, FileStorageService fileStorage) : ControllerBase
{
    private UserRole GetRole() =>
        Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(int orderId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        if (!OrderAccessHelper.CanAccessOrderChat(order, userId, GetRole()))
            return Forbid();

        var messages = await db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Where(m => m.OrderId == orderId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages.Select(m => m.ToDto(fileStorage)).ToList());
    }

    [HttpPost("order/{orderId}")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<MessageDto>> SendMessage(
        int orderId,
        [FromForm] string? text,
        [FromForm] List<IFormFile>? files)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        if (!OrderAccessHelper.CanAccessOrderChat(order, userId, GetRole()))
            return Forbid();

        var hasText = !string.IsNullOrWhiteSpace(text);
        var hasFiles = files != null && files.Count > 0;

        if (!hasText && !hasFiles)
            return BadRequest(new { message = "Сообщение не может быть пустым" });

        var message = new Message
        {
            OrderId = orderId,
            SenderId = userId,
            Text = text?.Trim() ?? string.Empty
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        if (hasFiles)
        {
            foreach (var file in files!)
            {
                try
                {
                    var (storedName, contentType, size) = await fileStorage.SaveAsync(file);
                    db.Attachments.Add(new Attachment
                    {
                        FileName = file.FileName,
                        StoredName = storedName,
                        ContentType = contentType,
                        Size = size,
                        UploadedById = userId,
                        MessageId = message.Id,
                        OrderId = orderId
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            await db.SaveChangesAsync();
        }

        await db.Entry(message).Reference(m => m.Sender).LoadAsync();
        await db.Entry(message).Collection(m => m.Attachments).LoadAsync();
        return Ok(message.ToDto(fileStorage));
    }
}
