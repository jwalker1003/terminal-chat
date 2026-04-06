namespace ChatClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to Terminal Chat!/n/n");
            Console.WriteLine("Please enter your username:");

            string? userName = string.Empty;

            while (string.IsNullOrEmpty(userName))
            {
                userName = Console.ReadLine();
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("Username cannot be empty. Exiting.");
                    return;
                }
            }

            // Main read loop is here
            await ServerConnection.ConnectToServer(userName);
        }
    }
}