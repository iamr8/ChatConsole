using System.Net;

using ChatService.Base;

using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace ChatService.Tests
{
    [Order(1)]
    public class SocketTests
    {
        private const string hostname = "localhost";
        private const string serverUser = "Assist";
        private const string clientUser = "iamr8";
        private readonly IPEndPoint _endPoint;

        public SocketTests(ITestOutputHelper output)
        {
            Console.SetOut(new ConsoleWriter(output));
            _endPoint = SocketBase.GetEndPointAsync(hostname).GetAwaiter().GetResult();
        }

        [Fact, Order(1)]
        public async Task Should_SendByClient_ReceiveByServer()
        {
            using (var s = await SocketServer.CreateConnectionAsync(hostname))
            {
                s.Start(serverUser);

                using (var c = await SocketClient.CreateConnectionAsync(hostname))
                {
                    c.Connect(clientUser);
                    c.Send("Hello there");

                    await Task.Delay(1000);

                    // Assert
                    Assert.NotEmpty(c.Messages);
                    Assert.NotEmpty(s.Messages);

                    Assert.Contains(s.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Received);
                    Assert.Contains(c.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Sent);
                }
            }
        }

        [Fact, Order(2)]
        public async Task Should_SendByServer_ReceiveByClient()
        {
            using (var s = await SocketServer.CreateConnectionAsync(hostname))
            {
                s.Start(serverUser);

                using (var c = await SocketClient.CreateConnectionAsync(hostname))
                {
                    c.Connect(clientUser);

                    await Task.Delay(1000);

                    s.Send("Hello there");

                    // Assert
                    Assert.NotEmpty(c.Messages);
                    Assert.NotEmpty(s.Messages);

                    Assert.Contains(s.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Sent);
                    Assert.Contains(c.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Received);
                }
            }
        }

        [Fact, Order(3)]
        public async Task Should_KickClient_ForAccessViolation()
        {
            using (var s = await SocketServer.CreateConnectionAsync(hostname))
            {
                s.Start(serverUser);

                using (var c = await SocketClient.CreateConnectionAsync(hostname))
                {
                    c.Connect(clientUser);
                    c.Send("Hello there");
                    await Task.Delay(100);
                    c.Send("Hello there");
                    await Task.Delay(100);
                    c.Send("Hello there");
                    c.Send("Hello there");

                    // Assert
                    Assert.False(c.IsConnected);
                }
            }
        }
    }
}