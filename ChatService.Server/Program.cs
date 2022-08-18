using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();

using var socket = await SocketServer.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
socket.Start("Assist");

var message = Console.ReadLine();
socket.Send(message);

Console.ReadKey();