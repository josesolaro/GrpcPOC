using Chat;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace TestCode
{
    public class ChatImp : Chat.Chat.ChatBase
    {
        public override Task Room(IAsyncStreamReader<Message> requestStream, IServerStreamWriter<Message> responseStream, ServerCallContext context)
        {
            var listenerTask = Task.Run(async () => await Listener(requestStream));
            var senderTask = Task.Run(async () => await Sender(responseStream));
            Task.WhenAny(new Task[] { listenerTask, senderTask }).Wait();
            return Task.FromResult(true);
        }
        private async Task Listener(IAsyncStreamReader<Message> requestStream)
        {
            var lastMessage = "";
            while (lastMessage != "exit" && await requestStream.MoveNext())
            {
                var currMessage = requestStream.Current;
                System.Console.WriteLine($"{currMessage.Name}: {currMessage.Msg}");
                lastMessage = currMessage.Msg;
            };
        }
        private async Task Sender(IServerStreamWriter<Message> responseStream)
        {
            var message = "";
            while (message != "exit")
            {
                message = Console.ReadLine();
                await responseStream.WriteAsync(new Message() { Name = "Server", Msg = message });
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                const int port = 30052;
                Server server = new Server()
                {
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) },
                    Services = { Chat.Chat.BindService(new ChatImp()) }
                };
                System.Console.WriteLine("Server Start");
                server.Start();
                System.Console.ReadKey();
                server.ShutdownAsync().Wait();
            }
        }
    }
}