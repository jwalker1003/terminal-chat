namespace ChatServer.Events;

public class UserJoinedChannelEventArgs
{
    public string UserName { get; set; }
    public string ChannelName { get; set; }

    public UserJoinedChannelEventArgs(string userName, string channelName)
    {
        UserName = userName;
        ChannelName = channelName;
    }
}
