namespace Chat.Core.Entities;

public class Message(string content, User sender)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = content;
    public User Sender { get; set; } = sender;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
