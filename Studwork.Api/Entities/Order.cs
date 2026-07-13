namespace Studwork.Api.Entities;



public class Order

{

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public WorkType WorkType { get; set; }

    public string Subject { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    public DateTime Deadline { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Open;

    public bool IsPrivate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public int CustomerId { get; set; }

    public User Customer { get; set; } = null!;



    public int? ExecutorId { get; set; }

    public User? Executor { get; set; }



    public int? InvitedExecutorId { get; set; }

    public User? InvitedExecutor { get; set; }



    public ICollection<Bid> Bids { get; set; } = [];

    public ICollection<Message> Messages { get; set; } = [];

    public ICollection<Attachment> Attachments { get; set; } = [];

    public Review? Review { get; set; }

}


