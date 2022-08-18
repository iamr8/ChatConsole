using ChatService.Base;

using var cancellationTokenSource = new CancellationTokenSource();

Console.WriteLine("Please enter an alias to connect to an assist.");
var name = Console.ReadLine();

using var socket = await SocketClient.CreateConnectionAsync("localhost", cancellationTokenSource.Token);
socket.Connect(name);
//await socket.ConnectAsync(cancellationTokenSource.Token);
//socket.OnReceived += (sender, eventArgs) =>
//{
//    Console.WriteLine("Server> {0}", eventArgs.Message);
//};

//var line = Console.ReadLine();
//if (!string.IsNullOrEmpty(line))
//    await socket.SendAsync(line, cancellationTokenSource.Token);

var message = Console.ReadLine();
socket.Send(message);

var f = Console.ReadKey();