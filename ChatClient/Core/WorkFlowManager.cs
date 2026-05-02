using System.Net.Sockets;
using System.Text;
using Chat.Core.Common;

namespace ChatClient.Core;

public class WorkflowManager(TcpSession tcpSession, UserActionManager userActionManager)
{
    private readonly TcpSession _tcpSession = tcpSession;
    private readonly UserActionManager _userActionManager = userActionManager;

    public async Task StartWorkflow()
    {
        if (!Initialize().IsSuccess)
        {
           return;
        }

        // Login
        // TODO: make a User type instead of passing back the username as string. This would also manage user auth
        var loginResult = LoginUser();
        if (!loginResult.IsSuccess)
        {
            Console.WriteLine($"Failed to login. {loginResult.ErrorMessage}");
            return;
        }

        // Connect to server
        Result<TcpClient?> connectResult = await _tcpSession.ConnectToServer(); 
        if (!connectResult.IsSuccess)
        {
            Console.WriteLine("Failed to connect to server.");
            return;
        }
        if (connectResult.Value == null)
        {
            Console.WriteLine("Network stream is null after successful connection.");
            return;
        }
        using var tcpClient = connectResult.Value;
        using var networkStream = tcpClient.GetStream();

        // Server expects user registration data immediately upon connection
        // TODO: Ultimately this would be an access token or some other form of authentication, but for now just the username
        // would this also be part of connecting to the server? Like a handshake that includes authentication/authorization info and other metadata about the client?
        var registerResult = await TcpSession.RegisterUsername(networkStream, loginResult.Value ?? "");
        if (!registerResult.IsSuccess)
        {
            Console.WriteLine($"Failed to register username. {registerResult.ErrorMessage}");
            return;
        }

        // Wait for user to input command to create/join a channel before starting main read/write loops
        var commandResult = WaitForInitialCommand();
        if (!commandResult.IsSuccess)
        {
            Console.WriteLine($"Failed to read user command. {commandResult.ErrorMessage}");
            return;
        }
        if (string.IsNullOrEmpty(commandResult.Value))
        {
            Console.WriteLine("Command cannot be empty.");
            return;
        }
        string command = commandResult.Value;  

        // Validate command before sending to server, but for now just assume user inputs correctly
        var sendResult = await TcpSession.SendCommand(networkStream, command);
        if (!sendResult.IsSuccess)
        {
            Console.WriteLine($"Failed to send command. {sendResult.ErrorMessage}");
            return;
        }
        if (sendResult.Value == null || sendResult.Value.Length == 0)
        {
            Console.WriteLine("No response from server.");
            return;
        }

        string responseStr = Encoding.UTF8.GetString(sendResult.Value);
        Console.WriteLine(responseStr);

        // Would check that we joined a channel successfully before starting to read/write messages, 
        // but for now just assume it works and move on to the main read/write loops
        // TODO: load X amount of previous messages
        // This will only read new messages - server won't send messages that were sent before we joined the channel, so we won't have to worry about that for now
        var readTask = TcpSession.ReadMessages(networkStream);
        var inputTask = _userActionManager.HandleUserInput();

        await Task.WhenAny(readTask, inputTask);
    }

    private Result Initialize()
    {
        try
        {
            _userActionManager.SendMessage += _tcpSession.SendMessage;
            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to initialize WorkflowManager: {e.Message}");
            return Result.Failure();
        }
    }

    // Temp method to get username from user input before connecting to server
    private static Result<string> LoginUser()
    {
        Console.WriteLine("Welcome to Terminal Chat!/n/n");
        Console.WriteLine("Please enter your username:");

        string? userName = string.Empty;

        while (string.IsNullOrEmpty(userName))
        {
            userName = Console.ReadLine();
            if (string.IsNullOrEmpty(userName))
                return Result<string>.Failure("Username cannot be empty.");
        }

        return Result<string>.Success(userName); 
    }

    private static Result<string> WaitForInitialCommand()
    {
        try
        {
            bool waitingForCommands = true;
            string? command = string.Empty;
            while (waitingForCommands)
            {
                // Start a loop to wait for action to create/join a channel. Creating will automatically join
                Console.WriteLine("Enter a command to create or join a channel (e.g. '/create-channel MyChannel' or '/join-channel MyChannel'):");
                command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                {
                    Console.WriteLine("Command cannot be empty. Please try again.");
                    continue;
                }
                waitingForCommands = false;
            }
            return Result<string>.Success(command ?? "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error waiting for commands: {ex.GetType().Name} - {ex.Message}");
            return Result<string>.Failure("Failed to read user command.");  
        }
    }
}
