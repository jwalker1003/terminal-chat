using Chat.Core.Common;
using Chat.Core.Entities;
using ChatServer.Entities;

namespace ChatServer.Core;

public class UserActionManager(ChannelManager channelManager)
{
    private readonly ChannelManager channelManager = channelManager;    

    public Result<string> HandleUserAction(string command, string[] args, ClientConnection client)
    {
        // TODO: Use strategy pattern here 
        switch (command)
        {
            case "/create-channel":
                if (args.Length != 2)
                {
                    return Result<string>.Failure("Failed to create channel. Number of arguments is less than 2.");
                }

                // Would check and validate args here, but for now just assume they're correct
                var newChannel = new Channel(args[0], args[1]);
                channelManager.AddChannel(newChannel);
                channelManager.JoinChannel(client, newChannel);
                client.CurrentChannelId = newChannel.Id;
                return Result<string>.Success($"Created new channel: {args[0]}");

            case "/join-channel":
                if (args.Length >= 1)
                {
                    // Would check and validate args here, but for now just assume they're correct
                    var channel = channelManager.GetChannelByName(args[0]);
                    if (channel == null)
                    {
                        return Result<string>.Failure($"Channel '{args[0]}' does not exist.");
                    }
                    channelManager.JoinChannel(client, channel);
                    client.CurrentChannelId = channel.Id;
                    return Result<string>.Success($"Joined channel: {args[0]}");
                }
                break;
            case "/leave-channel":
                if (args.Length >= 1)
                {
                    // Would check and validate args here, but for now just assume they're correct
                    var channel = channelManager.GetChannelByName(args[0]);
                    if (channel == null)
                    {
                        return Result<string>.Failure($"Channel '{args[0]}' does not exist.");
                    }
                    channelManager.LeaveChannel(client, channel);
                    return Result<string>.Success($"Left channel: {args[0]}");
                }
                break;  

            case "/list-channels":
                var channels = channelManager.Channels;
                string channelString = string.Empty;
                if (channels.Count == 0)
                {
                    Console.WriteLine("No channels available.");
                }
                else
                {
                    Console.WriteLine("Available channels:");
                    
                    foreach (var channel in channels)
                    {
                        channelString += $"- {channel.Name}: {channel.Description}\n";
                        Console.WriteLine(channelString);
                    }
                }
                return Result<string>.Success(channelString);

            default: 
                return Result<string>.Failure($"Unknown command: {command}");
        }
        return Result<string>.Failure($"Unknown error parsing command");
    }
}
