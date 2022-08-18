using System.Net;
using System.Net.Sockets;

namespace ChatService.Base;

/// <summary>
/// An abstract class that pre-initializes some parts of a socket.
/// </summary>
public abstract class SocketBase : IDisposable
{
    /// <summary>
    /// An endpoint that represents the socket's end url.
    /// </summary>
    protected readonly IPEndPoint _endpoint;

    /// <summary>
    /// The socket that has duty to send and receive messages for the current user, either server, or client.
    /// </summary>
    protected readonly Socket Listener;

    /// <summary>
    /// Default constructor that initializes the socket.
    /// </summary>
    /// <param name="endpoint">An <see cref="IPEndPoint"/> object that represents the end url of the socket.</param>
    protected SocketBase(IPEndPoint endpoint)
    {
        // The given endpoint which is set by the coder, should be stored as a field to be usable at any time.
        _endpoint = endpoint;

        // Declare a socket, according to the given endpoint address, based on TCP protocol.
        Listener = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Announcement of current status of the socket, which is now just started.
        this.Log($"Socket started on {_endpoint}", LogLevel.Information);
    }

    protected void Log(string message, LogLevel level = 0)
    {
        Console.Write("\n[{0:T}] ", DateTime.Now);
        switch (level)
        {
            case LogLevel.Information:
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                break;

            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                break;

            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                break;

            case LogLevel.Text:
            default:
                break;
        }

        Console.Write(" {0}\n", message);
        Console.ResetColor();
    }

    ///// <summary>
    ///// A delegate type to let the coder declare an event for the time when a message received.
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="args"></param>
    //public delegate void OnReceivedDelegate(object sender, SocketMessageEventArgs args);

    ///// <summary>
    ///// A event that would be invoked if current socket receives any message from the other nodes of the network.
    ///// </summary>
    //public virtual event OnReceivedDelegate OnReceived;

    ///// <summary>
    ///// A request to connect to an existing socket.
    ///// </summary>
    ///// <returns></returns>
    //public abstract void Connect();

    ///// <summary>
    ///// Sends a message through TCP protocol in socket to the socket, which is receivable for the other users.
    ///// </summary>
    ///// <param name="text"></param>
    ///// <returns></returns>
    //public abstract void Send(string text);

    /// <summary>
    /// Returns a <see cref="IPEndPoint"/> object according to the given host name.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected static async Task<IPEndPoint> GetEndPointAsync(string host, CancellationToken cancellationToken = default)
    {
        var hostEntry = await Dns.GetHostEntryAsync(host, cancellationToken);
        var ip = hostEntry.AddressList[0];
        var localEndPoint = new IPEndPoint(ip, 11000);
        return localEndPoint;
    }

    /// <summary>
    /// Disposes object when no need to be used anymore.
    /// </summary>
    public virtual void Dispose()
    {
        Listener.Dispose();
    }
}