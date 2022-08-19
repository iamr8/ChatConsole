using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();

using var socket = await SocketServer.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
socket.Start("Assist");

while (true)
{
    var message = Console.ReadLine();

    if (message.Contains("-e"))
        break;

    socket.Send(message);
}

Console.ReadKey();