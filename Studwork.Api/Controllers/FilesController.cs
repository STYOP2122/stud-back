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
public class FilesController(AppDbContext db, FileStorageService fileStorage) : ControllerBase
{
    private UserRole GetRole() =>
        Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<List<AttachmentDto>>> GetOrderFiles(int orderId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();
        if (!OrderAccessHelper.CanView(order, userId, GetRole())) return Forbid();

        var files = await db.Attachments
            .Where(a => a.OrderId == orderId && a.MessageId == null)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        return Ok(files.Select(f => f.ToDto(fileStorage)).ToList());
    }

    [HttpPost("order/{orderId}")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<AttachmentDto>> UploadOrderFile(int orderId, IFormFile file)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();
        if (!OrderAccessHelper.CanUploadOrderFiles(order, userId, GetRole()))
            return Forbid();

        try
        {
            var (storedName, contentType, size) = await fileStorage.SaveAsync(file);
            var attachment = new Attachment
            {
                FileName = file.FileName,
                StoredName = storedName,
                ContentType = contentType,
                Size = size,
                UploadedById = userId,
                OrderId = orderId
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();
            return Ok(attachment.ToDto(fileStorage));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = GetRole();

        var attachment = await db.Attachments
            .Include(a => a.Order)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null) return NotFound();

        if (role != UserRole.Admin && attachment.UploadedById != userId)
            return Forbid();

        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
