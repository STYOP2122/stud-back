namespace Studwork.Api.Entities;



public class DirectMessage

{

    public int Id { get; set; }

    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public int SenderId { get; set; }

    public User Sender { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public ICollection<Attachment> Attachments { get; set; } = [];

}


