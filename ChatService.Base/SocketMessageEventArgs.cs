namespace ChatService.Base;

public class SocketMessageEventArgs : EventArgs
{
    /// <summary>
    /// The message that is received through the socket.
    /// </summary>
    public string Message { get; init; }
}