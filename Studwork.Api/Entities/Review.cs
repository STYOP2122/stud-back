namespace Studwork.Api.Entities;

public class Review
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    public int ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
