using System.Text;

namespace ChatServer;

public static class DisplayFormatter
{
    /// <summary>
    /// Formats a message for display by prepending the sender's username to the message content.
    /// </summary>
    /// <param name="message">The message content as a byte array.</param>
    /// <param name="senderUserName">The username of the sender.</param>
    /// <returns>byte[]</returns>
    /// <example> 
    /// "{Username}: {message content}"
    /// </example>
    public static byte[] FormatMessageForDisplay(byte[] message, string senderUserName)
        => [.. Encoding.UTF8.GetBytes($"[{senderUserName}]: "), .. message];
}
