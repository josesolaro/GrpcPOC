using Chat;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static private async Task Listener(AsyncDuplexStreamingCall<Message, Message> call)
        {
            var msg = "";
            while (msg != "exit" && await call.ResponseStream.MoveNext())
            {
                var curr = call.ResponseStream.Current;
                Console.WriteLine($"{curr.Name}: {curr.Msg}");
                msg = curr.Msg;
            }
        }
        static private async Task Sender(AsyncDuplexStreamingCall<Message, Message> call)
        {
            var msg = "";
            while (msg != "exit")
            {
                msg = Console.ReadLine();
                await call.RequestStream.WriteAsync(new Message() { Name = "Client", Msg = msg });
            }
        }
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:30052", ChannelCredentials.Insecure);
            var client = new Chat.Chat.ChatClient(channel);
            System.Console.WriteLine("Client Start");
            using (var call = client.Room())
            {
                var listenerTask = Task.Run(async () => await Listener(call));
                var senderTask = Task.Run(async () => await Sender(call));
                Task.WhenAny(new Task[] { listenerTask, senderTask }).Wait();
                call.RequestStream.CompleteAsync().Wait();
            }
        }
    }
}
