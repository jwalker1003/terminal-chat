using Chat.Core.Common;
using Chat.Core.Entities;
using ChatServer.Entities;

namespace ChatServer.Core;

public class ChannelManager
{
    public List<Channel> Channels { get; set; } = [];
    public Dictionary<Guid, IList<ClientConnection>> ChannelConnections { get; set; } = [];

    public Channel? GetChannelByName(string name)
        =>  Channels.FirstOrDefault(c => c.Name == name);
    public Channel? GetChannelById(Guid id)
        =>  Channels.FirstOrDefault(c => c.Id == id);

    public IEnumerable<ClientConnection> GetClientsInChannel(Guid id)
        => ChannelConnections.TryGetValue(id, out var clients) ? clients : [];

    public Result AddChannel(Channel channel)
    {
        try
        {
            // TODO: Don't manage both, at least not here 
            Channels.Add(channel);
            ChannelConnections.Add(channel.Id, []);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while adding the channel: {ex.Message}");
        }
    }

    public Result JoinChannel(ClientConnection client, Channel channel)
    {
        try
        {
            if (ChannelConnections.TryGetValue(channel.Id, out IList<ClientConnection>? value))
            {
                value.Add(client);
                return Result.Success();
            }

            return Result.Failure();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while joining the channel: {ex.Message}");
        }
    }

    public Result LeaveChannel(ClientConnection client, Channel channel)
    {
        try
        {
            if (ChannelConnections.TryGetValue(channel.Id, out IList<ClientConnection>? value))
            {
                value.Remove(client);
                Result.Success();
            }

            return Result.Failure();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while leaving the channel: {ex.Message}");
        }
    }   

    public Result RemoveChannel(Channel channel)
    {
        try
        {
            Channels.Remove(channel);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while removing the channel: {ex.Message}");
        }
    }
}
