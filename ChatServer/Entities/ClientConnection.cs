using System.Net.Sockets;
using Chat.Core.Entities;

namespace ChatServer.Entities
{
    public class ClientConnection(TcpClient tcpClient, string userName = "Unknown")
    {
        public Guid Id { get; } = Guid.NewGuid();
        public NetworkStream Stream { get; } = tcpClient.GetStream();
        public User User { get; } = new User(userName); // This would normally be populated VIA a database
        public Guid? CurrentChannelId { get; set; }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}