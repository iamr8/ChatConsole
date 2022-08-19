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

                    await Task.Delay(100);

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

        [Fact, Order(4)]
        public async Task Should_Be_A_Conversation()
        {
            using (var s = await SocketServer.CreateConnectionAsync(hostname))
            {
                s.Start(serverUser);

                using (var c = await SocketClient.CreateConnectionAsync(hostname))
                {
                    c.Connect(clientUser);
                    c.Send("Hello there");
                    await Task.Delay(1001);
                    s.Send("Hi. How can i help you?");
                    await Task.Delay(1001);
                    c.Send("I'd like to know further about your pricing plans.");
                    await Task.Delay(1001);
                    s.Send("Sure. I can help you with that.");

                    // Assert
                    Assert.NotEmpty(c.Messages);
                    Assert.NotEmpty(s.Messages);

                    Assert.Contains(c.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Sent);
                    Assert.Contains(s.Messages, msg => msg.Message == "Hello there" && msg.State == SocketMessageState.Received);

                    Assert.Contains(s.Messages, msg => msg.Message == "Hi. How can i help you?" && msg.State == SocketMessageState.Sent);
                    Assert.Contains(c.Messages, msg => msg.Message == "Hi. How can i help you?" && msg.State == SocketMessageState.Received);

                    Assert.Contains(c.Messages, msg => msg.Message == "I'd like to know further about your pricing plans." && msg.State == SocketMessageState.Sent);
                    Assert.Contains(s.Messages, msg => msg.Message == "I'd like to know further about your pricing plans." && msg.State == SocketMessageState.Received);

                    Assert.Contains(s.Messages, msg => msg.Message == "Sure. I can help you with that." && msg.State == SocketMessageState.Sent);
                    Assert.Contains(c.Messages, msg => msg.Message == "Sure. I can help you with that." && msg.State == SocketMessageState.Received);
                }
            }
        }
    }
}