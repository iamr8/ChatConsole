namespace ChatService.Base;

public record SocketBacklog(SocketMessageState State, string Alias, string Message, DateTime Created);