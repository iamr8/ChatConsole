using System.Net;
using System.Text;

namespace ChatService.Base;

public record SocketClient : SocketBase
{
    public override event OnReceivedDelegate OnReceived;
    private SocketClient(IPEndPoint endpoint) : base(endpoint)
    {
    }

    public static async Task<SocketClient> CreateConnectionAsync(string host, CancellationToken cancellationToken = default)
    {
        var endPoint = await GetEndPointAsync(host, cancellationToken);
        return new SocketClient(endPoint);
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        string data = null;
        var bytes = new byte[Sender.ReceiveBufferSize];

        try
        {
            while (true)
            {
                var bytesReceived = Sender.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

                if (data.IndexOf("-e") > -1)
                    break;
            }

            OnReceived?.Invoke(this, new SocketMessageEventArgs { Message = data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public override async Task SendAsync(string text, CancellationToken cancellationToken = default)
    {
        var msg = Encoding.UTF8.GetBytes(text);
        Sender.Send(msg);
    }
}