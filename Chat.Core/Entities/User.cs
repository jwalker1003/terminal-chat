namespace Chat.Core.Entities;

public class User(string username)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Username { get; set; } = username;
}
