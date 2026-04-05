using System.Net.Sockets;

namespace Chat.Infrastructure;

public static class MessageFramer
{
    public static byte[] PrefixLength(byte[] data)
    {
        try
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            byte[] prefixedData = new byte[lengthPrefix.Length + data.Length];
            Buffer.BlockCopy(lengthPrefix, 0, prefixedData, 0, lengthPrefix.Length);
            Buffer.BlockCopy(data, 0, prefixedData, lengthPrefix.Length, data.Length);
            return prefixedData;
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Invalid argument in PrefixLength: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in PrefixLength: {ex}");
            throw;
        }       
    }

    public static async Task<byte[]> ReadMessageAsync(NetworkStream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] lengthBuffer = new byte[4];
            await stream.ReadExactlyAsync(lengthBuffer, cancellationToken);
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] messageBuffer = new byte[messageLength];
            await stream.ReadExactlyAsync(messageBuffer, cancellationToken);
            return messageBuffer;
        }
        catch (EndOfStreamException)
        {
            // Connection closed gracefully
            return [];
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx && 
                                     (socketEx.SocketErrorCode == SocketError.ConnectionReset || 
                                      socketEx.SocketErrorCode == SocketError.ConnectionAborted))
        {
            // Connection dropped unexpectedly - treat as graceful disconnection
            return [];
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in ReadMessageAsync: {ex}");
            throw;
        }
    }

    public static async Task WriteMessageAsync(NetworkStream stream, byte[] message, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] prefixedMessage = PrefixLength(message);
            await stream.WriteAsync(prefixedMessage, cancellationToken);
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx && 
                                     (socketEx.SocketErrorCode == SocketError.ConnectionReset || 
                                      socketEx.SocketErrorCode == SocketError.ConnectionAborted))
        {
            // Connection dropped unexpectedly
            Console.WriteLine($"Connection lost while writing: {ex.Message}");
            throw;
        }
        catch (ObjectDisposedException)
        {
            // Stream has been disposed
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
