using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService.Base;

public class SocketClient : SocketBase
{
    private string _alias;

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
        _alias = alias;

        try
        {
            this.Listener.BeginConnect(_endpoint, ConnectCallback, this.Listener);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void Send(Socket handler, string message, bool shadowSend = false)
    {
        try
        {
            var msgBytes = Encoding.ASCII.GetBytes(message);
            var state = new SocketState
            {
                Handler = handler,
                Bag = new Dictionary<string, string>() { { "shadow", "true" } },
                Buffer = msgBytes,
                BufferSize = msgBytes.Length
            };
            handler.BeginSend(state.Buffer, 0, state.BufferSize, 0, SendCallback, state);
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

            this.Log($"You've been connected to an assist.", LogLevel.Information);

            var msg = $"::alias={_alias}";
            Send(client, msg, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            var state = (SocketState)ar.AsyncState;
            var handler = state.Handler;

            var bytesSent = handler.EndSend(ar);

            if (!state.Bag.TryGetValue("shadow", out _))
                this.Log($"Your message has been sent. ({bytesSent} bytes)", LogLevel.Information);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}