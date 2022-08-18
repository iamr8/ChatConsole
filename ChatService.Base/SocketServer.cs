using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService.Base;

public class SocketServer : SocketBase
{
    public SocketServer(IPEndPoint endPoint) : base(endPoint)
    {
    }

    public static async Task<SocketServer> CreateConnectionAsync(string host, CancellationToken cancellationToken = default)
    {
        var hostEntry = await GetEndPointAsync(host, cancellationToken);
        return new SocketServer(hostEntry);
    }

    public void StartListening()
    {
        this.Listener.Bind(_endpoint);
        this.Listener.Listen(10);

        try
        {
            this.Listener.BeginAccept(AcceptCallback, this.Listener);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        var listener = (Socket)ar.AsyncState!;
        var handler = listener.EndAccept(ar);

        this.Log($"A client has been connected to you.", LogLevel.Information);

        var state = new SocketState();
        state.Buffer = new byte[handler.ReceiveBufferSize];
        state.BufferSize = handler.ReceiveBufferSize;
        state.Handler = handler;
        handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, ReceiveCallback, state);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        var state = (SocketState)ar.AsyncState;
        var handler = state.Handler;
        var bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            var msg = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
            if (msg.StartsWith("::"))
            {
                // internal msg
                var arr = msg[2..].Split("=");
                var key = arr[0];
                var value = arr[1];
                state.Bag.TryAdd(key, value);
            }
            else
            {
                Console.WriteLine("{0:T} - {1}: {2}", DateTime.Now, state.Bag["alias"], msg);
            }
        }
    }
}