namespace Studwork.Api.Entities;



public class Message

{

    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int SenderId { get; set; }

    public User Sender { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public ICollection<Attachment> Attachments { get; set; } = [];

}


