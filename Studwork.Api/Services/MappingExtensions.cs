using Microsoft.EntityFrameworkCore;
using Studwork.Api.Data;
using Studwork.Api.DTOs;
using Studwork.Api.Entities;

namespace Studwork.Api.Services;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Email,
        user.Name,
        user.Role,
        user.Bio,
        user.AvatarUrl,
        user.Phone,
        user.Skills,
        user.PortfolioUrl,
        user.City,
        user.IsPro,
        user.IsBanned,
        user.Rating,
        user.CompletedOrders,
        user.CreatedAt
    );

    public static AdminUserDto ToAdminDto(this User user) => new(
        user.Id,
        user.Email,
        user.Name,
        user.Role,
        user.IsPro,
        user.IsBanned,
        user.Rating,
        user.CompletedOrders,
        user.CreatedAt
    );

    public static AttachmentDto ToDto(this Attachment attachment, FileStorageService storage) => new(
        attachment.Id,
        attachment.FileName,
        storage.GetUrl(attachment.StoredName),
        attachment.ContentType,
        attachment.Size,
        storage.IsImage(attachment.ContentType),
        attachment.CreatedAt
    );

    public static BidDto ToDto(this Bid bid) => new(
        bid.Id,
        bid.Price,
        bid.Message,
        bid.DaysToComplete,
        bid.IsAccepted,
        bid.CreatedAt,
        bid.Executor.ToDto()
    );

    public static MessageDto ToDto(this Message message, FileStorageService storage) => new(
        message.Id,
        message.Text,
        message.CreatedAt,
        message.Sender.ToDto(),
        message.Attachments?.Select(a => a.ToDto(storage)).ToList() ?? []
    );

    public static DirectMessageDto ToDto(this DirectMessage message, FileStorageService storage) => new(
        message.Id,
        message.Text,
        message.CreatedAt,
        message.Sender.ToDto(),
        message.Attachments?.Select(a => a.ToDto(storage)).ToList() ?? []
    );

    public static ReviewDto ToDto(this Review review) => new(
        review.Id,
        review.Rating,
        review.Comment,
        review.CreatedAt,
        review.FromUser.ToDto()
    );

    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.Title,
        order.Description,
        order.WorkType,
        order.Subject,
        order.Budget,
        order.Deadline,
        order.Status,
        order.IsPrivate,
        order.InvitedExecutor?.ToDto(),
        order.CreatedAt,
        order.Customer.ToDto(),
        order.Executor?.ToDto(),
        order.Bids?.Count ?? 0
    );

    public static async Task<OrderDetailDto> ToDetailDtoAsync(this Order order, AppDbContext db, FileStorageService storage)
    {
        var bids = await db.Bids
            .Include(b => b.Executor)
            .Where(b => b.OrderId == order.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var attachments = await db.Attachments
            .Where(a => a.OrderId == order.Id && a.MessageId == null)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        return new OrderDetailDto(
            order.Id,
            order.Title,
            order.Description,
            order.WorkType,
            order.Subject,
            order.Budget,
            order.Deadline,
            order.Status,
            order.IsPrivate,
            order.InvitedExecutor?.ToDto(),
            order.CreatedAt,
            order.Customer.ToDto(),
            order.Executor?.ToDto(),
            bids.Select(b => b.ToDto()).ToList(),
            attachments.Select(a => a.ToDto(storage)).ToList(),
            order.Review != null ? order.Review.ToDto() : null
        );
    }
}
