using ChatClient.Events;

namespace ChatClient.Core;

public class UserActionManager
{
    public event EventHandler<SendMessageEventArgs>? SendMessage;

    protected virtual void OnSendMessage(string message)
        => SendMessage?.Invoke(this, new SendMessageEventArgs(message));

    public async Task HandleUserInput()
    {
        
        try
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    continue;

                if (input == ":q") break;

                OnSendMessage(input);
            }
        }
        catch (Exception)
        {
            Console.WriteLine();   
        }
        
    }
}
