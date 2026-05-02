namespace Chat.Core.Common;

public static class UserActionHelper
{
    // Trivial but works for now 
    public static bool IsUserAction(string str) => str.StartsWith("/");
}
