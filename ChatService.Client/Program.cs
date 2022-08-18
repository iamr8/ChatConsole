using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();
using var socket = await SocketClient.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
await socket.ConnectAsync(cancellationTokenSource.Token);
socket.OnReceived += (sender, eventArgs) =>
{
    Console.WriteLine("Server> {0}", eventArgs.Message);
};

var line = Console.ReadLine();
if (!string.IsNullOrEmpty(line))
    await socket.SendAsync(line, cancellationTokenSource.Token);