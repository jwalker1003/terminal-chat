namespace ChatClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to Terminal Chat!/n/n");

            // Main read loop is here
            await ServerConnection.ConnectToServer();
        }
    }
}