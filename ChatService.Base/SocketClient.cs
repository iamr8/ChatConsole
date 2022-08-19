using System.Net;
using System.Net.Sockets;

namespace ChatService.Base;

public class SocketClient : SocketBase
{
    protected int WrongAttempts { get; private set; }

    public SocketClient(IPEndPoint endpoint) : base(endpoint)
    {
    }

    public static SocketClient CreateConnection(string host)
    {
        var hostEntry = GetEndPoint(host);
        return new SocketClient(hostEntry);
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
            this.Log(e.Message, LogLevel.Error);
        }
    }

    public void LeaveConversation()
    {
        if (this.Listener is { Connected: true })
        {
            this.Log("You are about to leave this conversation. Farewell!", LogLevel.Information);
            try
            {
                this.Listener.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                this.Log(e.Message, LogLevel.Error);
            }
        }
    }

    public override void Send(string message)
    {
        if (this.Listener is { Connected: true })
        {
            Send(this.Listener,
                message,
                beforeSend: () =>
                {
                    var lastMessage = this.Messages
                        .Where(x => x.State == SocketMessageState.Sent)
                        .MaxBy(x => x.Created);

                    if (lastMessage != null)
                    {
                        var threshold = lastMessage.Created.AddSeconds(1).TimeOfDay.TotalMilliseconds;
                        var now = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        if (now < threshold)
                        {
                            this.WrongAttempts++;
                            switch (this.WrongAttempts)
                            {
                                case 1:
                                    this.Log($"You are sending messages too fast. Please wait a little bit. You are allowed to send one message per second.", LogLevel.Warning);
                                    break;

                                case > 1:
                                    this.Log("Unfortunately, we have to inform you, we closed this thread in order to rules violations.", LogLevel.Error);
                                    this.LeaveConversation();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        this.WrongAttempts = 0;
                    }
                });
        }
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
            this.Log(e.Message, LogLevel.Error);
        }
    }
}