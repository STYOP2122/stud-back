namespace Studwork.Api.Entities;



public class Attachment

{

    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    public int UploadedById { get; set; }

    public User UploadedBy { get; set; } = null!;



    public int? OrderId { get; set; }

    public Order? Order { get; set; }



    public int? MessageId { get; set; }

    public Message? Message { get; set; }



    public int? DirectMessageId { get; set; }

    public DirectMessage? DirectMessage { get; set; }

}


