using Chat.Core.Communication;
using Chat.Core.Enums;
using ChatServer.Entities;
using ChatServer.Core;
using System.Text;
using ChatServer.Events;

namespace ChatServer.Connections.TCP;

internal class TcpClientService(UserActionManager userActionManager)
    : IDisposable
{
    private readonly UserActionManager userActionManager = userActionManager;

    private static readonly List<ClientConnection> clientGroup = [];
    private static readonly SemaphoreSlim clientGroupLock = new(1, 1);

    private CancellationTokenSource? _cts;
    private bool isDisposed;

    public event EventHandler<BroadcastMessageEventArgs>? BroadcastMessageEvent;

    public void ReadClientAsync(ClientConnection client, CancellationToken cancellationToken)
    {
        try
        {
            Task.Run(async () =>
            {
                var dispatcher = new MessageDispatcher();
                dispatcher.Register(MessageType.Command, data => HandleCommandAsync(data, client, cancellationToken));
                dispatcher.Register(MessageType.Message, data => HandleChatMessageAsync(data, client));
                await dispatcher.RunAsync(client.Stream, cancellationToken);
                Console.WriteLine($"Client {client.Id} disconnected.");
            }, cancellationToken);
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine($"Client {client.Id} connection was disposed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ReadClientAsync for {client.Id}: {ex.GetType().Name} - {ex.Message}");
        }
    }

    private async Task HandleCommandAsync(byte[] data, ClientConnection client, CancellationToken cancellationToken)
    {
        var incomingMessage = Encoding.UTF8.GetString(data);
        var args = incomingMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = userActionManager.HandleUserAction(args[0], [.. args.Skip(1)], client);

        string response = result.IsSuccess
            ? result.Value ?? string.Empty
            : $"ERROR: {result.ErrorMessage}";

        byte[] outboundMessage = Encoding.UTF8.GetBytes(response);
        await MessageFramer.WriteMessageAsync(client.Stream, outboundMessage, MessageType.Command, cancellationToken);
    }

    private Task HandleChatMessageAsync(byte[] data, ClientConnection client)
    {
        if (client.CurrentChannelId is not Guid channelId)
        {
            Console.WriteLine($"[{client.User.Username}] sent a message but is not in a channel.");
            return Task.CompletedTask;
        }
        var incomingMessage = $"[{client.User.Username}]: {Encoding.UTF8.GetString(data)}";
        Console.WriteLine(incomingMessage);
        BroadcastMessageEvent?.Invoke(this, new BroadcastMessageEventArgs(channelId, incomingMessage, client.Id));
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        _cts?.Cancel();
        _cts?.Dispose();
    }
}
