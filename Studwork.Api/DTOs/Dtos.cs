using Studwork.Api.Entities;



namespace Studwork.Api.DTOs;



public record RegisterRequest(string Email, string Password, string Name, UserRole Role);



public record LoginRequest(string Email, string Password);



public record AuthResponse(string Token, UserDto User);



public record UserDto(

    int Id,

    string Email,

    string Name,

    UserRole Role,

    string? Bio,

    string? AvatarUrl,

    string? Phone,

    string? Skills,

    string? PortfolioUrl,

    string? City,

    bool IsPro,

    bool IsBanned,

    double Rating,

    int CompletedOrders,

    DateTime CreatedAt

);



public record UpdateProfileRequest(

    string Name,

    string? Bio,

    string? Phone,

    string? Skills,

    string? PortfolioUrl,

    string? City

);



public record AttachmentDto(

    int Id,

    string FileName,

    string Url,

    string ContentType,

    long Size,

    bool IsImage,

    DateTime CreatedAt

);



public record CreateOrderRequest(

    string Title,

    string Description,

    WorkType WorkType,

    string Subject,

    decimal Budget,

    DateTime Deadline,

    bool IsPrivate = false,

    int? InvitedExecutorId = null

);



public record OrderDto(

    int Id,

    string Title,

    string Description,

    WorkType WorkType,

    string Subject,

    decimal Budget,

    DateTime Deadline,

    OrderStatus Status,

    bool IsPrivate,

    UserDto? InvitedExecutor,

    DateTime CreatedAt,

    UserDto Customer,

    UserDto? Executor,

    int BidsCount

);



public record OrderDetailDto(

    int Id,

    string Title,

    string Description,

    WorkType WorkType,

    string Subject,

    decimal Budget,

    DateTime Deadline,

    OrderStatus Status,

    bool IsPrivate,

    UserDto? InvitedExecutor,

    DateTime CreatedAt,

    UserDto Customer,

    UserDto? Executor,

    List<BidDto> Bids,

    List<AttachmentDto> Attachments,

    ReviewDto? Review

);



public record CreateBidRequest(decimal Price, string Message, int DaysToComplete);



public record BidDto(

    int Id,

    decimal Price,

    string Message,

    int DaysToComplete,

    bool IsAccepted,

    DateTime CreatedAt,

    UserDto Executor

);



public record MessageDto(

    int Id,

    string Text,

    DateTime CreatedAt,

    UserDto Sender,

    List<AttachmentDto> Attachments

);



public record CreateReviewRequest(int Rating, string? Comment);



public record ReviewDto(

    int Id,

    int Rating,

    string? Comment,

    DateTime CreatedAt,

    UserDto FromUser

);



public record OrderFilter(

    OrderStatus? Status = null,

    WorkType? WorkType = null,

    string? Subject = null,

    string? Search = null

);



public record ConversationDto(

    int Id,

    UserDto OtherUser,

    string? LastMessageText,

    DateTime LastMessageAt

);



public record DirectMessageDto(

    int Id,

    string Text,

    DateTime CreatedAt,

    UserDto Sender,

    List<AttachmentDto> Attachments

);



public record AdminStatsDto(

    int TotalUsers,

    int TotalOrders,

    int OpenOrders,

    int ProUsers,

    int BannedUsers

);



public record AdminUserDto(

    int Id,

    string Email,

    string Name,

    UserRole Role,

    bool IsPro,

    bool IsBanned,

    double Rating,

    int CompletedOrders,

    DateTime CreatedAt

);



public record SetProRequest(bool IsPro);



public record SetBanRequest(bool IsBanned);


