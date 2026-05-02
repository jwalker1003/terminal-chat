using System.Text;
using Chat.Core.Communication;
using Chat.Core.Enums;
using ChatServer.Entities;

namespace ChatServer.Connections.TCP;

public class TcpBroadcaster
{
    public static async Task BroadcastAsync(string message, IEnumerable<ClientConnection> clients, CancellationToken cancellationToken)
    {
        foreach (var client in clients)
        {
            try
            {
                await MessageFramer.WriteMessageAsync(client.Stream, Encoding.UTF8.GetBytes(message), MessageType.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error broadcasting to client {client.Id}: {ex.Message}");
            }
        }
    }
}
