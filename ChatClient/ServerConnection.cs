using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class ServerConnection
    {
        public async Task ConnectToServer()
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                using TcpClient tcpClient = new();
                tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 9090); // Make configurable
                Console.WriteLine("Connecting to server!");

                var stream = tcpClient.GetStream();
                var readTask = ReadMessages(stream);
                var writeTask = WriteMessages(stream);

                await Task.WhenAny(readTask, writeTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public async Task ReadMessages(NetworkStream stream)
        {
            try
            {

                await Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024];
                    while (true)
                    {
                        _ = await stream.ReadAsync(buffer);
                        var msg = Encoding.UTF8.GetString(buffer);
                        Console.WriteLine(msg);
                    }
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task WriteMessages(NetworkStream stream)
        {
            try
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == ":q") break;
                    byte[] buffer = Encoding.UTF8.GetBytes(input ?? "");
                    await stream.WriteAsync(buffer).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}