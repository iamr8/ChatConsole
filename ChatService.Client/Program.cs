using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();

Console.WriteLine("Please enter an alias to connect to an assist.");
var name = Console.ReadLine();

using var socket = await SocketClient.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
socket.Connect(name);

while (true)
{
    var message = Console.ReadLine();

    if (message.Contains("-e"))
        break;

    socket.Send(message);
}

Console.ReadKey();