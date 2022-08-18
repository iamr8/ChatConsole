using System.Net;
using System.Net.Sockets;

namespace ChatService.Base;

public abstract record SocketBase : IDisposable
{
    protected readonly IPEndPoint _endpoint;
    protected readonly Socket Sender;

    protected SocketBase(IPEndPoint endpoint)
    {
        _endpoint = endpoint;

        Sender = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        Console.WriteLine("Socket started on {0}", _endpoint);
    }
    public delegate void OnReceivedDelegate(object sender, SocketMessageEventArgs args);

    public virtual event OnReceivedDelegate OnReceived;

    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);
    public abstract Task SendAsync(string text, CancellationToken cancellationToken = default);

    protected static async Task<IPEndPoint> GetEndPointAsync(string host, CancellationToken cancellationToken = default)
    {
        var hostEntry = await Dns.GetHostEntryAsync(host, cancellationToken);
        var ip = hostEntry.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        var localEndPoint = new IPEndPoint(ip, 11000);
        return localEndPoint;
    }
    public virtual void Dispose()
    {
        Sender.Dispose();
    }
}