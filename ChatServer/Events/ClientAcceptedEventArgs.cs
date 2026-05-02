using ChatServer.Entities;

namespace ChatServer.Events;

public class ClientAcceptedEventArgs(ClientConnection clientConnection)
{
    public ClientConnection ClientConnection { get; } = clientConnection;
}
