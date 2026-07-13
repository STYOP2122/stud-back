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
public class ConversationsController(AppDbContext db, FileStorageService fileStorage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var conversations = await db.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var result = new List<ConversationDto>();
        foreach (var conv in conversations)
        {
            var other = ConversationHelper.GetOtherUser(conv, userId);
            var lastMsg = await db.DirectMessages
                .Where(m => m.ConversationId == conv.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Text)
                .FirstOrDefaultAsync();

            result.Add(new ConversationDto(conv.Id, other.ToDto(), lastMsg, conv.LastMessageAt));
        }

        return Ok(result);
    }

    [HttpPost("with/{otherUserId}")]
    public async Task<ActionResult<ConversationDto>> GetOrCreate(int otherUserId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userId == otherUserId)
            return BadRequest(new { message = "Нельзя написать самому себе" });

        var other = await db.Users.FindAsync(otherUserId);
        if (other == null || other.IsBanned || other.Role == UserRole.Admin)
            return NotFound();

        var (u1, u2) = ConversationHelper.NormalizePair(userId, otherUserId);
        var conversation = await db.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.User1Id == u1 && c.User2Id == u2);

        if (conversation == null)
        {
            conversation = new Conversation { User1Id = u1, User2Id = u2 };
            db.Conversations.Add(conversation);
            await db.SaveChangesAsync();
            await db.Entry(conversation).Reference(c => c.User1).LoadAsync();
            await db.Entry(conversation).Reference(c => c.User2).LoadAsync();
        }

        var otherUser = ConversationHelper.GetOtherUser(conversation, userId);
        return Ok(new ConversationDto(conversation.Id, otherUser.ToDto(), null, conversation.LastMessageAt));
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetMessages(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversation = await db.Conversations.FindAsync(id);
        if (conversation == null) return NotFound();
        if (conversation.User1Id != userId && conversation.User2Id != userId) return Forbid();

        var messages = await db.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages.Select(m => m.ToDto(fileStorage)).ToList());
    }

    [HttpPost("{id}/messages")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<DirectMessageDto>> SendMessage(
        int id,
        [FromForm] string? text,
        [FromForm] List<IFormFile>? files)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversation = await db.Conversations.FindAsync(id);
        if (conversation == null) return NotFound();
        if (conversation.User1Id != userId && conversation.User2Id != userId) return Forbid();

        var hasText = !string.IsNullOrWhiteSpace(text);
        var hasFiles = files != null && files.Count > 0;
        if (!hasText && !hasFiles)
            return BadRequest(new { message = "Сообщение не может быть пустым" });

        var message = new DirectMessage
        {
            ConversationId = id,
            SenderId = userId,
            Text = text?.Trim() ?? string.Empty
        };

        db.DirectMessages.Add(message);
        conversation.LastMessageAt = DateTime.UtcNow;

        if (hasFiles)
        {
            await db.SaveChangesAsync();
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
                        DirectMessageId = message.Id
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
        }

        await db.SaveChangesAsync();

        await db.Entry(message).Reference(m => m.Sender).LoadAsync();
        await db.Entry(message).Collection(m => m.Attachments).LoadAsync();
        return Ok(message.ToDto(fileStorage));
    }
}
