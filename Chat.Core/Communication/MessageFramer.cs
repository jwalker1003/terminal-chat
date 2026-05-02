using System.Net.Sockets;
using Chat.Core.Enums;

namespace Chat.Core.Communication;

public static class MessageFramer
{
    public static async Task<(byte[], MessageType)> ReadMessageAsync(NetworkStream stream, CancellationToken ct = default)
    {
        try
        {
            byte[] lengthBuffer = new byte[4];
            byte[] typeBuffer = new byte[1];

            await stream.ReadExactlyAsync(lengthBuffer, ct);
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            await stream.ReadExactlyAsync(typeBuffer, ct);
            MessageType messageType = (MessageType)typeBuffer[0];

            byte[] messageBuffer = new byte[messageLength];
            await stream.ReadExactlyAsync(messageBuffer, ct);
            return (messageBuffer, messageType);
        }
        catch (EndOfStreamException)
        {
            return ([], MessageType.Unknown);
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                     (socketEx.SocketErrorCode == SocketError.ConnectionReset ||
                                      socketEx.SocketErrorCode == SocketError.ConnectionAborted))
        {
            return ([], MessageType.Unknown);
        }
        catch (OperationCanceledException)
        {
            return ([], MessageType.Unknown);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in ReadMessageAsync: {ex}");
            throw;
        }
    }

    public static async Task WriteMessageAsync(NetworkStream stream, byte[] message, MessageType type, CancellationToken ct = default)
    {
        try
        {
            // Frame layout: [4-byte length][1-byte type][payload]
            byte[] frame = new byte[4 + 1 + message.Length];
            BitConverter.GetBytes(message.Length).CopyTo(frame, 0);
            frame[4] = (byte)type;
            Buffer.BlockCopy(message, 0, frame, 5, message.Length);
            await stream.WriteAsync(frame, ct);
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                     (socketEx.SocketErrorCode == SocketError.ConnectionReset ||
                                      socketEx.SocketErrorCode == SocketError.ConnectionAborted))
        {
            Console.WriteLine($"Connection lost while writing: {ex.Message}");
            throw;
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Stream has been closed or disposed");
            throw;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Write operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in WriteMessageAsync: {ex}");
            throw;
        }
    }
}
