using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create a TCP Listener 
            using var listener = new TcpListener(IPAddress.Any, 9090);
            listener.Start();

            while (true)
            {
                // Get stream from client
                TcpClient client = await listener.AcceptTcpClientAsync();

                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                {
                    using NetworkStream stream = client.GetStream();
                    var buffer = new byte[1_024];

                    while (true)
                    {
                        int received = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                        if (received == 0)
                            break;

                        var message = Encoding.UTF8.GetString(buffer, 0, received);
                        Console.WriteLine(message);

                        if (stream.CanWrite)
                            await stream.WriteAsync(buffer.AsMemory(0, received));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
