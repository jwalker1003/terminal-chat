using System.Net.Sockets;
using Chat.Core.Entities;

namespace ChatServer.Entities
{
    internal class ClientConnection(TcpClient tcpClient, string userName = "Unknown")
    {
        public Guid Id { get; } = Guid.NewGuid();
        public NetworkStream Stream { get; } = tcpClient.GetStream();
        public User User { get; } = new User(userName); // This would normally be populated VIA a database

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}