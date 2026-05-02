using ChatServer.Core;

namespace ChatServer;

internal class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        using var workflow = new MainWorkflow();
        await workflow.Start(cts.Token);
    }
}