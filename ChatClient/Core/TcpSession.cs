using Chat.Core.Common;
using Chat.Core.Communication;
using Chat.Core.Enums;
using ChatClient.Events;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient.Core;

public class TcpSession
    : IDisposable
{
    public TcpClient TcpClient { get; private set; } = new();
    private NetworkStream? _stream;

    public bool isDisposed = false;

    public async Task<Result<TcpClient?>> ConnectToServer()
    {
        try
        {
            Console.WriteLine("Connecting to server...");
            TcpClient.Connect(IPAddress.Parse("127.0.0.1"), 9090);
            _stream = TcpClient.GetStream();
            Console.WriteLine("Connected to server!");
            return Result<TcpClient?>.Success(TcpClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
            return Result<TcpClient?>.Failure("Failed to connect to server.");
        }
    }

    public static async Task<Result<byte[]>> SendCommand(NetworkStream stream, string commandArgs)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(commandArgs);
            await MessageFramer.WriteMessageAsync(stream, buffer, MessageType.Command);

            var (data, _) = await MessageFramer.ReadMessageAsync(stream);
            return Result<byte[]>.Success(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending command: {ex.GetType().Name} - {ex.Message}");
            return Result<byte[]>.Failure("Failed to send command.");
        }
    }

    public static async Task<Result> RegisterUsername(NetworkStream stream, string userName)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(userName);
            await MessageFramer.WriteMessageAsync(stream, buffer, MessageType.Registration);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering username: {ex.GetType().Name} - {ex.Message}");
            return Result.Failure("Failed to register username.");
        }
    }

    public static async Task ReadMessages(NetworkStream stream)
    {
        try
        {
            var dispatcher = new MessageDispatcher();
            dispatcher.Register(MessageType.Message, data =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(data));
                return Task.CompletedTask;
            });
            dispatcher.Register(MessageType.Command, data =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(data));
                return Task.CompletedTask;
            });
            await dispatcher.RunAsync(stream);
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Connection to server was closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from server: {ex.GetType().Name} - {ex.Message}");
        }
        finally
        {
            stream.Close();
        }
    }

    public async void SendMessage(object? sender, SendMessageEventArgs e)
    {
        try
        {
            if (_stream == null) return;

            await MessageFramer.WriteMessageAsync(_stream, Encoding.UTF8.GetBytes(e.Message), MessageType.Message);
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                      (socketEx.SocketErrorCode == SocketError.ConnectionReset ||
                                       socketEx.SocketErrorCode == SocketError.ConnectionAborted))
        {
            Console.WriteLine("Connection to server lost.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to server: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        TcpClient.Dispose();
        _stream?.Dispose();
    }
}
