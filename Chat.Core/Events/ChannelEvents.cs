namespace Chat.Core.Events;

public class ChannelEvents
{
    public event EventHandler<ChannelCreatedEventArgs>? ChannelCreated;
}

public class ChannelCreatedEventArgs : EventArgs
{
    public string ChannelName { get; set; }

    public ChannelCreatedEventArgs(string channelName)
    {
        ChannelName = channelName;
    }
}

