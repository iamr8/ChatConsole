using System.Net;
using System.Net.Sockets;

namespace ChatService.Base;

public class SocketServer : SocketBase
{
    public Socket Handler { get; set; }

    public SocketServer(IPEndPoint endPoint) : base(endPoint)
    {
    }

    public static async Task<SocketServer> CreateConnectionAsync(string host, CancellationToken cancellationToken = default)
    {
        var hostEntry = await GetEndPointAsync(host, cancellationToken);
        return new SocketServer(hostEntry);
    }

    public void Start(string alias)
    {
        this.Alias = alias;
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
        Handler = listener.EndAccept(ar);
        this.Log($"A client has been connected to you.", LogLevel.Information);

        var state = new SocketState();
        state.Buffer = new byte[Handler.ReceiveBufferSize];
        state.BufferSize = Handler.ReceiveBufferSize;
        state.Handler = Handler;
        Handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, ReceiveCallback, state);
    }

    public void Send(string message)
    {
        if (this.Handler is not { Connected: true })
        {
            this.Log("Still no user is being connected.", LogLevel.Error);
            return;
        }

        Send(this.Handler, message);
    }

    public override void Dispose()
    {
        Handler.Shutdown(SocketShutdown.Both);
        Handler.Close();
    }
}