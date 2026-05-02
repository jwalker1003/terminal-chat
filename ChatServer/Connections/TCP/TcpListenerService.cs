using System.Net;
using System.Net.Sockets;
using System.Text;
using Chat.Core.Communication;
using ChatServer.Entities;
using ChatServer.Events;

namespace ChatServer.Connections.TCP;

public class TcpListenerService
    : IDisposable
{
    public bool IsListening { get; private set; } = false;
    public event EventHandler<ClientAcceptedEventArgs>? ClientAccepted;

    private CancellationTokenSource? acceptClientCancellationToken;
    private TcpListener? _listener;
    private bool _isDisposed = false;

    public bool StartTcpListener()
    {
        try
        {
            // Create a TCP Listener
            Console.WriteLine("Starting server...");
            _listener = new TcpListener(IPAddress.Any, 9090); // TODO: make port configurable?
            _listener.Start();
            Console.WriteLine("Server started.");
            IsListening = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            IsListening = false;
        }

        return IsListening;
    }

    public void AcceptClients(CancellationToken externalToken)
    {
        if (ClientAccepted == null)
            throw new NotImplementedException("ClientAccepted event handler is not set.");

        if (_listener == null)
            return; 

        Task.Run(async () =>
        {
            acceptClientCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            var cancellationToken = acceptClientCancellationToken.Token;

            while (!externalToken.IsCancellationRequested)
            {

                // Get stream from client
                TcpClient inClient = await _listener.AcceptTcpClientAsync(cancellationToken);

                // Immediately listen for user registration data
                // TODO: Ultimately this would be an access token or some other form of authentication, but for now just the username
                var (registrationData, _) = await MessageFramer.ReadMessageAsync(inClient.GetStream(), cancellationToken);
                if (registrationData.Length == 0)
                {
                    Console.WriteLine($"Client failed to register.");
                    continue;
                }

                string userName = Encoding.UTF8.GetString(registrationData, 0, registrationData.Length);

                OnClientAccepted(new ClientConnection(inClient, userName));
            }
        }, externalToken);

    }

    protected virtual void OnClientAccepted(ClientConnection clientConnection)
    {
        ClientAccepted?.Invoke(this, new ClientAcceptedEventArgs(clientConnection));
    }

    public void Dispose()
    {
        if (_isDisposed)
            return; 
        _isDisposed = true;

        _listener?.Dispose();
        acceptClientCancellationToken?.Cancel();
        acceptClientCancellationToken?.Dispose();
    }
}
