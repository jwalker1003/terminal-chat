using ChatServer.Models;
using System.Net;
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
        private static readonly SemaphoreSlim clientLock = new(1, 1);


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
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    var connection = new ClientConnection(client);
                    await clientLock.WaitAsync();
                    try
                    {
                        clientGroup.Add(connection);
                        Console.WriteLine($"Client {connection.Id} connected. Total clients: {clientGroup.Count}");
                    }
                    finally
                    {
                        clientLock.Release();
                    }

                    _ = LoopReadClientAsync(connection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task LoopReadClientAsync(ClientConnection connection)
        {
            try
            {
                var buffer = new byte[1_024];

                while (true)
                {
                    Console.WriteLine("Reading data...");
                    int received = await connection.Stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                    if (received == 0)
                    {
                        Console.WriteLine($"Client {connection.Id} disconnected.");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"[{connection.Id}] {DateTime.Now}: {message}");

                    await BroadCoastByteArray(buffer, received, connection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoopReadClientAsync for {connection.Id}: {ex.Message}");
            }
            finally
            {
                // Remove from client group and dispose
                await clientLock.WaitAsync();
                try
                {
                    clientGroup.RemoveAll(c => c.Id == connection.Id);
                    connection.Dispose();
                    Console.WriteLine($"Client {connection.Id} removed. Total clients: {clientGroup.Count}");          
                }
                finally
                {
                    clientLock.Release();
                }
            }
        }

        private async Task BroadCoastByteArray(byte[] buffer, int length, ClientConnection sender)
        {
            try
            {
                // Take a snapshot of client connections while locked
                List<ClientConnection> clientsToNotify;
                await clientLock.WaitAsync();
                try
                {
                    clientsToNotify = clientGroup
                        .Where(c => c.Id != sender.Id)
                        .ToList();
                }
                finally
                {
                    clientLock.Release();
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