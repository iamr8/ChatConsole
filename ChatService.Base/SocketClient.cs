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

    private void Send(Socket handler, string message)
    {
        try
        {
            var msgBytes = Encoding.ASCII.GetBytes(message);
            handler.BeginSend(msgBytes, 0, msgBytes.Length, 0, SendCallback, handler);
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

            Console.WriteLine("{0:T} - You've been connected to an assist.", DateTime.Now);

            var msg = $"::alias={_alias}";
            Send(client, msg);
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
            var client = (Socket)ar.AsyncState;

            var bytesSent = client.EndSend(ar);

            Console.WriteLine("{0:T} - Your message has been sent. ({1} bytes)", DateTime.Now, bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}