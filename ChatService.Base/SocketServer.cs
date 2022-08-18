using System.Net;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;

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

    private void ReceiveCallback(IAsyncResult ar)
    {
        var state = (SocketState)ar.AsyncState;
        var handler = state.Handler;
        var bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // state.Messages.AppendLine(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
            var msg = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
            // var msg = state.Messages.ToString();
            if (msg.IndexOf("<EOF>") > -1)
            {
                msg = msg.Split("<EOF>")[0];
                var obj = JsonConvert.DeserializeObject<SocketMessage>(msg, this.JsonSerializer);
                if (!obj.Internal)
                    this.Log($"{obj.User}: {obj.Message}");
            }
            else
            {
                handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, ReceiveCallback, state);
            }
        }
    }
}