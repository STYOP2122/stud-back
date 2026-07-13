namespace Studwork.Api.Entities;



public class User

{

    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Phone { get; set; }

    public string? Skills { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? City { get; set; }

    public bool IsPro { get; set; }

    public bool IsBanned { get; set; }

    public double Rating { get; set; }

    public int CompletedOrders { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public ICollection<Order> CreatedOrders { get; set; } = [];

    public ICollection<Order> AssignedOrders { get; set; } = [];

    public ICollection<Order> InvitedOrders { get; set; } = [];

    public ICollection<Bid> Bids { get; set; } = [];

    public ICollection<Message> Messages { get; set; } = [];

    public ICollection<Review> ReviewsGiven { get; set; } = [];

    public ICollection<Review> ReviewsReceived { get; set; } = [];

    public ICollection<Attachment> Attachments { get; set; } = [];

    public ICollection<DirectMessage> DirectMessages { get; set; } = [];

}


