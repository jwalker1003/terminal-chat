using System.Net.Sockets;
using System.Text;

namespace Chat.Infrastructure;

public static class MessageFramer
{
    public static byte[] PrefixLength(byte[] data)
    {
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
        byte[] prefixedData = new byte[lengthPrefix.Length + data.Length];
        Buffer.BlockCopy(lengthPrefix, 0, prefixedData, 0, lengthPrefix.Length);
        Buffer.BlockCopy(data, 0, prefixedData, lengthPrefix.Length, data.Length);
        return prefixedData;
    }

    public static async Task<byte[]> ReadMessageAsync(NetworkStream stream, CancellationToken cancellationToken = default)
    {
        byte[] lengthBuffer = new byte[4];
        await stream.ReadExactlyAsync(lengthBuffer, cancellationToken);
        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

        byte[] messageBuffer = new byte[messageLength];
        await stream.ReadExactlyAsync(messageBuffer, cancellationToken);
        return messageBuffer;
    }

    public static async Task WriteMessageAsync(NetworkStream stream, byte[] message, CancellationToken cancellationToken = default)
    {
        byte[] prefixedMessage = PrefixLength(message);
        await stream.WriteAsync(prefixedMessage, cancellationToken);
    }
}
