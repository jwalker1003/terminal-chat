using Chat.Infrastructure;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class ServerConnection
    {
        public static async Task ConnectToServer()
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                using TcpClient tcpClient = new();
                tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 9090); // Make configurable
                Console.WriteLine("Connecting to server!");

                var stream = tcpClient.GetStream();
                var readTask = ReadMessages(stream);
                var writeTask = HandleUserInput(stream);

                await Task.WhenAny(readTask, writeTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private static async Task ReadMessages(NetworkStream stream)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = await MessageFramer.ReadMessageAsync(stream);                      
                    var msg = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task HandleUserInput(NetworkStream stream)
        {
            try
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == null)
                        continue;

                    if (input == ":q") break;
                    
                    byte[] buffer = Encoding.UTF8.GetBytes(input ?? "");

                    await MessageFramer.WriteMessageAsync(stream, buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}