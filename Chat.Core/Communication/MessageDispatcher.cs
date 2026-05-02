using System.Net.Sockets;
using Chat.Core.Enums;

namespace Chat.Core.Communication;

public class MessageDispatcher
{
    private readonly Dictionary<MessageType, Func<byte[], Task>> _handlers = [];

    public void Register(MessageType type, Func<byte[], Task> handler)
        => _handlers[type] = handler;

    public async Task RunAsync(NetworkStream stream, CancellationToken ct = default)
    {
        while (true)
        {
            var (data, type) = await MessageFramer.ReadMessageAsync(stream, ct);
            if (data.Length == 0) break;

            if (_handlers.TryGetValue(type, out var handler))
                await handler(data);
            else
                Console.WriteLine($"No handler registered for message type: {type}");
        }
    }
}
