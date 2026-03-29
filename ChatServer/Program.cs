namespace ChatServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var clientManager = new ClientManager();
            _ = await clientManager.StartTcpListener();
        }
    }
}
