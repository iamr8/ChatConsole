using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();

Console.WriteLine("Please enter an alias to connect to an assist.");
var name = Console.ReadLine();

using var socket = await SocketClient.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
socket.Connect(name);

var message = Console.ReadLine();
socket.Send(message);

Console.ReadKey();