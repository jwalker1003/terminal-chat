namespace ChatServer.Events;

public class BroadcastMessageEventArgs(Guid _channelId, string _message, Guid _senderId)
{
    public Guid ChannelId { get; set; } = _channelId;
    public string Message { get; set; } = _message;
    public Guid SenderId { get; set; } = _senderId;
}
