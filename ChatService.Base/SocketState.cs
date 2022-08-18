using System.Net.Sockets;

namespace ChatService.Base;

internal class SocketState
{
    public byte[] Buffer { get; set; }
    public Socket Handler { get; set; }
    public int BufferSize { get; set; }
    public Dictionary<string, string> Bag { get; set; } = new();
}