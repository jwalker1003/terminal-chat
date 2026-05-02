using Chat.Core.Entities;
using ChatClient.Core;

namespace ChatClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Would likely want to add user configuration and other setup here eventually, but for now just start the session
            var session = new WorkflowManager(new TcpSession(), new UserActionManager());
            await session.StartWorkflow();
        }
    }
}