namespace Studwork.Api.Entities;

public class Bid
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ExecutorId { get; set; }
    public User Executor { get; set; } = null!;
    public decimal Price { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DaysToComplete { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
