namespace ChatService.Base;

public class SocketMessageEventArgs : EventArgs
{
    public string Message { get; init; }
}