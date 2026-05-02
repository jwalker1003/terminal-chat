using ChatServer.Events;

namespace ChatServer.Interfaces;

public interface IUserActionHandler
{
    public event EventHandler<UserCreatedChannelEventArgs> UserCreatedChannel;
    public event EventHandler<UserJoinedChannelEventArgs> UserJoinedChannel;
}
