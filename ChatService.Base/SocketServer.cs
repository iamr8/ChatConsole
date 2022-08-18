using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService.Base;

public record SocketServer : SocketBase
{
    public override event OnReceivedDelegate OnReceived;
    private Socket Client { get; set; }

    private SocketServer(IPEndPoint endpoint) : base(endpoint)
    {
        Sender.Bind(_endpoint);
        Sender.Listen(10);

        Console.WriteLine("Waiting for a client...");
    }

    public static async Task<SocketServer> CreateConnectionAsync(string host, CancellationToken cancellationToken = default)
    {
        var endPoint = await GetEndPointAsync(host, cancellationToken);
        return new SocketServer(endPoint);
    }
    private void CallAccept(IAsyncResult iar)
    {
        var server = (Socket)iar.AsyncState;
        var client = server.EndAccept(iar);
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        Client = await Sender.AcceptAsync(cancellationToken);

        Console.WriteLine("Client joined.");

        string data = null;
        var bytes = new byte[Client.ReceiveBufferSize];

        try
        {
            while (true)
            {
                var bytesReceived = Client.Receive(bytes);
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
        Client.Send(msg);
    }

    public override void Dispose()
    {
        Client.Shutdown(SocketShutdown.Both);
        Client.Close();
        Client.Dispose();
    }
}