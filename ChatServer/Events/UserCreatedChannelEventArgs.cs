namespace ChatServer.Events;

public class UserCreatedChannelEventArgs
{
    public string UserName { get; set; }
    public string ChannelName { get; set; }

    public UserCreatedChannelEventArgs(string userName, string channelName)
    {
        UserName = userName;
        ChannelName = channelName;
    }
}
