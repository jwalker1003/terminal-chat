using ChatServer.Models;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    internal class ClientManager
    {
        /// <summary>
        /// Collection of currently connected clients
        /// </summary>
        private static readonly List<ClientConnection> clientGroup = [];

        /// <summary>
        /// Synchronizes access to the client collection over more than one thread
        /// </summary>
        private static readonly SemaphoreSlim clientGroupLock = new(1, 1);


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task StartTcpListener()
        {
            try
            {
                // Create a TCP Listener
                Console.WriteLine("Starting server...");
                using var listener = new TcpListener(IPAddress.Any, 9090); // TODO: make port configurable?
                listener.Start();
                Console.WriteLine("Server started.");

                while (true)
                {
                    // Get stream from client
                    TcpClient inClient = await listener.AcceptTcpClientAsync();
                    var client = new ClientConnection(inClient);
                    await clientGroupLock.WaitAsync();
                    try
                    {
                        clientGroup.Add(client);
                        Console.WriteLine($"Client {client.Id} connected. Total clients: {clientGroup.Count}");
                    }
                    finally
                    {
                        clientGroupLock.Release();
                    }

                    _ = LoopReadClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task LoopReadClientAsync(ClientConnection client)
        {
            try
            {
                var buffer = new byte[1_024];

                while (true)
                {
                    Console.WriteLine("Reading data...");
                    int received = await client.Stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                    if (received == 0)
                    {
                        Console.WriteLine($"Client {client.Id} disconnected.");
                        break;
                    }

                    // Copy the buffer to a new one to prevent overwriting or corrupted data when broadcasting
                    var messageBuffer = new byte[received];
                    Buffer.BlockCopy(buffer, 0, messageBuffer, 0, received);

                    var message = Encoding.UTF8.GetString(messageBuffer, 0, received);
                    Console.WriteLine($"[{client.Id}] {DateTime.Now}: {message}");

                    await BroadcastByteArray(messageBuffer, received, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoopReadClientAsync for {client.Id}: {ex.Message}");
            }
            finally
            {
                // Remove from client group and dispose
                await clientGroupLock.WaitAsync();
                try
                {
                    clientGroup.RemoveAll(c => c.Id == client.Id);
                    client.Dispose();
                    Console.WriteLine($"Client {client.Id} removed. Total clients: {clientGroup.Count}");          
                }
                finally
                {
                    clientGroupLock.Release();
                }
            }
        }

        private async Task BroadcastByteArray(byte[] buffer, int length, ClientConnection sender)
        {
            try
            {
                // Take a snapshot of client connections while locked
                List<ClientConnection> clientsToNotify;
                await clientGroupLock.WaitAsync();
                try
                {
                    clientsToNotify = clientGroup
                        .Where(c => c.Id != sender.Id)
                        .ToList();
                }
                finally
                {
                    clientGroupLock.Release();
                }

                // Broadcast outside the lock
                foreach (var client in clientsToNotify)
                {
                    try
                    {
                        await client.Stream.WriteAsync(buffer.AsMemory(0, length));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to client {client.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BroadCoastByteArray: {ex.Message}");
            }
        }
    }
}