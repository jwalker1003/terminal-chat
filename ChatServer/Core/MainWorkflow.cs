using ChatServer.Connections.TCP;
using ChatServer.Entities;
using ChatServer.Events;
using System.Linq;

namespace ChatServer.Core;

public class MainWorkflow
    : IDisposable
{
    private CancellationToken cancellationToken;
    private readonly IList<ClientConnection> clientConnections = [];
    private readonly TcpClientService tcpClientService;
    private readonly ChannelManager channelManager;
    private readonly TcpBroadcaster tcpBroadcaster;
    private readonly CancellationTokenSource clientAcceptedTokenSource = new();
    private bool isDisposed = false;

    public MainWorkflow()
    {
        channelManager =  new();
        tcpBroadcaster = new();
        tcpClientService = new(new UserActionManager(channelManager));
        tcpClientService.BroadcastMessageEvent += HandleBroadcast;
    }

    public async Task Start(CancellationToken token)
    {
        cancellationToken = token;
        using TcpListenerService listenerService = new();
        listenerService.StartTcpListener();
        if (listenerService.IsListening)
        {
            listenerService.ClientAccepted += HandleClientAccepted;
            listenerService.AcceptClients(cancellationToken);
        }
        await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
    }

    private void HandleClientAccepted(object? sender, ClientAcceptedEventArgs e)
    {
        clientConnections.Add(e.ClientConnection);
        tcpClientService?.ReadClientAsync(e.ClientConnection, clientAcceptedTokenSource.Token);
    }

    private void HandleBroadcast(object? sender, BroadcastMessageEventArgs e)
    {
        var channelClients = channelManager.GetClientsInChannel(e.ChannelId)
            .Where(c => c.Id != e.SenderId);
        _ = TcpBroadcaster.BroadcastAsync(e.Message, channelClients, cancellationToken);
    }

    public void Dispose()
    {
        if (isDisposed)
            return;
        isDisposed = true; 

        tcpClientService?.Dispose();

        clientAcceptedTokenSource.Cancel();
        clientAcceptedTokenSource.Dispose();
    }
}