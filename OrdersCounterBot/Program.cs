using System.Net;
using Telegram.Bot;

namespace OrdersCounterBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Run();
        }

        private static async Task Run()
        {
            //var listener = new HttpListener();
            //var port = "8080";
            //listener.Prefixes.Add($"http://*:{port}/");
            //listener.Start();

            //Console.WriteLine($"Listening on port {port}...");

            var apiToken = GetApiToken();
            Console.WriteLine("Getting api token");
            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(apiToken);
            var handler = new BotHandler(GetUserDataStorage(), new CommandParser());

            Console.WriteLine("Create handler");
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cts.Token);
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            //try
            //{
            //    await Task.Delay(-1, cts.Token);
            //}
            //catch (TaskCanceledException)
            //{
            //    Console.WriteLine("Bot stopped.");
            //}
            Console.WriteLine("EXIT????");
            cts.Cancel();
            //listener.Stop();
        }
        private static UserDataStorage GetUserDataStorage()
        {
            string? filePath;
            if (Environment.GetEnvironmentVariable("SERVER_ENV") == "true")
            {
                filePath = "/secrets/data.json";
            }
            else
            {
                filePath = "data.json";
            }
            Console.WriteLine($"Getting user data storage {filePath}");
            return new UserDataStorage(filePath);
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
