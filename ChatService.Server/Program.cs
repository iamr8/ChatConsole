using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();
using var socket = await SocketServer.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
await socket.ConnectAsync(cancellationTokenSource.Token);
await socket.SendAsync("Server> Hello, I am your assist. How can I help you?", cancellationTokenSource.Token);
socket.OnReceived += (sender, eventArgs) =>
{
    Console.WriteLine("Client> {0}", eventArgs.Message);
};

var line = Console.ReadLine();
if (!string.IsNullOrEmpty(line))
    await socket.SendAsync(line, cancellationTokenSource.Token);