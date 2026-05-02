namespace ChatClient.Events;

public class SendMessageEventArgs(string message)
    : EventArgs
{
    public string Message = message; 
}
