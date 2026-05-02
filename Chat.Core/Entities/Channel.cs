namespace Chat.Core.Entities;

public class Channel(string name, string description)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public IList<User> Users { get; } = [];
    public IList<Message> Messages { get; } = [];
}
