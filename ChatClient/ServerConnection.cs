using Chat.Core.Communication;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class ServerConnection
    {
        public static async Task ConnectToServer(string userName)
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                using TcpClient tcpClient = new();
                tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 9090); // Make configurable

                Console.WriteLine("Connected to server!");

                var stream = tcpClient.GetStream();

                // Server expects user registration data immediately upon connection
                // TODO: Ultimately this would be an access token or some other form of authentication, but for now just the username
                await RegisterUsername(stream, userName);

                var readTask = ReadMessages(stream);
                var writeTask = HandleUserInput(stream);

                await Task.WhenAny(readTask, writeTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        // Temp method to send username to server immediately upon connection
        private static async Task RegisterUsername(NetworkStream stream, string userName)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(userName);
            await MessageFramer.WriteMessageAsync(stream, buffer);
            
        }

        private static async Task ReadMessages(NetworkStream stream)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = await MessageFramer.ReadMessageAsync(stream);
                    if (buffer.Length == 0)
                    {
                        Console.WriteLine("Server disconnected.");
                        break;
                    }
                    var msg = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(msg);
                }
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

                    try
                    {
                        await MessageFramer.WriteMessageAsync(stream, buffer);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException socketEx && 
                                                  (socketEx.SocketErrorCode == SocketError.ConnectionReset || 
                                                   socketEx.SocketErrorCode == SocketError.ConnectionAborted))
                    {
                        Console.WriteLine("Connection to server lost.");
                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Connection to server was closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to server: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                stream.Close();
            }
        }
    }
}