using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class ServerConnection
    {
        public async Task ConnectToServer()
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

        public async Task ReadMessages(NetworkStream stream)
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

        public async Task WriteMessages(NetworkStream stream)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == ":q") break;
                byte[] buffer = Encoding.UTF8.GetBytes(input ?? "");
                await stream.WriteAsync(buffer).ConfigureAwait(false);
            }
        }
    }
}