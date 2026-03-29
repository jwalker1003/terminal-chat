using System.Net.Sockets;

namespace ChatServer.Models
{
    internal class ClientConnection
    {
        public NetworkStream Stream { get; }
        public Guid Id { get; }

        public ClientConnection(TcpClient tcpClient)
        {
            Stream = tcpClient.GetStream();
            Id = Guid.NewGuid();
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}