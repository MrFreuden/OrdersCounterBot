using System.Net;
using Telegram.Bot;

namespace OrdersCounterBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var apiToken = GetApiToken();
            Console.WriteLine("Getting api token");
            Run(apiToken);
        }

        private static void Run(string apiToken)
        {
            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(apiToken);
            var updateProcessor = new UpdateMessageProcessor(
                new UserDataStorage(UserDataStorage.GetDefaultPath()), new MessageSender(), new CommandProcessor(new CommandParser()));
            var handler = new BotHandler(bot, updateProcessor);
            Console.WriteLine("Created handler");

            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cts.Token);

            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            Console.WriteLine("EXIT????");
            cts.Cancel();
        }
      
        private static string GetApiToken()
        {
            DotNetEnv.Env.Load();
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN_FOR_TESTS");
            if (apiToken == null) throw new ArgumentNullException(apiToken);
            return apiToken;
        }
    }
}
