using System.Net.NetworkInformation;
using Chat.Core.Common;
using Chat.Core.Entities;   

namespace Chat.Core.Services;

public static class ChannelService
{
    public static Result<Channel> CreateChannel(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Channel>.Failure("Channel name cannot be empty.");
        }

        var channel = new Channel(name, description);
        return Result<Channel>.Success(channel);
    }

    public static Result DeleteChannel(Channel channel)
    {
        if (channel == null)
        {
            return Result.Failure("Channel cannot be null.", new ArgumentNullException(nameof(channel)));
        }

        return Result.Success();
    }
}
