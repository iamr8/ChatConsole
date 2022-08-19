using System.Net;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChatService.Base;

/// <summary>
/// An abstract class that pre-initializes some parts of a socket.
/// </summary>
public abstract class SocketBase : IDisposable
{
    public IEnumerable<SocketBacklog> Messages { get; private set; } = new List<SocketBacklog>();
    public bool IsConnected => this.Listener.Connected;

    /// <summary>
    /// An endpoint that represents the socket's end url.
    /// </summary>
    protected readonly IPEndPoint _endpoint;

    public string Alias { get; protected set; }

    /// <summary>
    /// The socket that has duty to send and receive messages for the current user, either server, or client.
    /// </summary>
    public readonly Socket Listener;

    /// <summary>
    /// Default constructor that initializes the socket.
    /// </summary>
    /// <param name="endpoint">An <see cref="IPEndPoint"/> object that represents the end url of the socket.</param>
    /// <param name="alias"></param>
    protected SocketBase(IPEndPoint endpoint)
    {
        // The given endpoint which is set by the coder, should be stored as a field to be usable at any time.
        _endpoint = endpoint;

        // Declare a socket, according to the given endpoint address, based on TCP protocol.
        Listener = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Announcement of current status of the socket, which is now just started.
        this.Log($"Socket started on {_endpoint}", LogLevel.Information);
    }

    public void Log(string message, LogLevel level = 0)
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

    protected JsonSerializerSettings JsonSerializer => new()
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    protected void SendCallback(IAsyncResult ar)
    {
        try
        {
            var state = (SocketState)ar.AsyncState;
            var handler = state.Handler;

            var bytesSent = handler.EndSend(ar);

            state.OnAfterSent?.Invoke();

            if (!state.Message.Internal)
            {
                this.Messages = this.Messages.Concat(new[] { new SocketBacklog(SocketMessageState.Sent, state.Message.User, state.Message.Message, DateTime.Now) });
                this.Log($"Your message has been sent. ({bytesSent} bytes)", LogLevel.Information);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public abstract void Send(string message);

    protected void Send(Socket handler, string message, Action beforeSend = null, Action afterSent = null, bool shadowSend = false)
    {
        try
        {
            if (message.Contains('<') || message.Contains('>'))
            {
                this.Log("Please avoid using < or > in your message.", LogLevel.Error);
                return;
            }

            beforeSend?.Invoke();
            if (handler is not { Connected: true })
            {
                this.Log("Socket is not connected.", LogLevel.Error);
                return;
            }

            var obj = new SocketMessage
            {
                Message = message,
                User = this.Alias,
                Internal = shadowSend,
            };

            var json = JsonConvert.SerializeObject(obj, this.JsonSerializer);
            json += "<EOF>";

            var msgBytes = Encoding.ASCII.GetBytes(json);
            var state = new SocketState
            {
                Handler = handler,
                Message = obj,
                Buffer = msgBytes,
                BufferSize = msgBytes.Length,
                OnAfterSent = afterSent
            };

            handler.BeginSend(state.Buffer, 0, state.BufferSize, 0, SendCallback, state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    protected void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            var state = (SocketState)ar.AsyncState;
            var handler = state.Handler;
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                var msg = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                if (msg.IndexOf("<EOF>") > -1)
                {
                    msg = msg.Split("<EOF>")[0];
                    var obj = JsonConvert.DeserializeObject<SocketMessage>(msg, this.JsonSerializer);
                    if (!obj.Internal)
                    {
                        this.Messages = this.Messages.Concat(new[] { new SocketBacklog(SocketMessageState.Received, obj.User, obj.Message, DateTime.Now) });
                        this.Log($"{obj.User}: {obj.Message}");
                    }
                }
                else
                {
                    handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, ReceiveCallback, state);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Returns a <see cref="IPEndPoint"/> object according to the given host name.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IPEndPoint GetEndPoint(string host)
    {
        var hostEntry = Dns.GetHostEntry(host);
        var ip = hostEntry.AddressList[0];
        var localEndPoint = new IPEndPoint(ip, 11000);
        return localEndPoint;
    }

    /// <summary>
    /// Returns a <see cref="IPEndPoint"/> object according to the given host name.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IPEndPoint> GetEndPointAsync(string host, CancellationToken cancellationToken = default)
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