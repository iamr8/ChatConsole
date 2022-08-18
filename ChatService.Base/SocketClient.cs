using System.Net;
using System.Net.Sockets;

namespace ChatService.Base;

public class SocketClient : SocketBase
{
    public SocketClient(IPEndPoint endpoint) : base(endpoint)
    {
    }

    public static async Task<SocketClient> CreateConnectionAsync(string host, CancellationToken cancellationToken = default)
    {
        var hostEntry = await GetEndPointAsync(host, cancellationToken);
        return new SocketClient(hostEntry);
    }

    public void Connect(string alias)
    {
        this.Alias = alias;

        try
        {
            this.Listener.BeginConnect(_endpoint, ConnectCallback, this.Listener);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Send(string message)
    {
        Send(this.Listener, message);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            var client = (Socket)ar.AsyncState!;
            client.EndConnect(ar);

            this.Log($"You've been successfully connected to an assist.", LogLevel.Information);

            var state = new SocketState();
            state.Buffer = new byte[client.ReceiveBufferSize];
            state.BufferSize = client.ReceiveBufferSize;
            state.Handler = client;
            client.BeginReceive(state.Buffer, 0, state.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}