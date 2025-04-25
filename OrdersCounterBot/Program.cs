using System.Net;
using System.Threading;
using Telegram.Bot;

namespace OrdersCounterBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var apiToken = GetApiToken();
            Console.WriteLine("Getting api token");
            await Run(apiToken);
        }

        private static string GetApiToken()
        {
            DotNetEnv.Env.Load();
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
            if (apiToken == null) throw new ArgumentNullException(apiToken);
            return apiToken;
        }

        private static async Task Run(string apiToken)
        {
            using var cts = new CancellationTokenSource();
            var listener = ConfigureHttpListener();
            var bot = ConfigureBot(apiToken, cts);

            var httpTask = HandleHttpRequestsAsync(listener, cts);
            var shutdownTask = HandleShutdownAsync(cts);

            try
            {
                await Task.WhenAny(httpTask, shutdownTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Application is shutting down...");
            }
            finally
            {
                StopServices(listener, cts);
            }
            Console.WriteLine("All services stopped.");
        }

        private static HttpListener ConfigureHttpListener()
        {
            var listener = new HttpListener();
            var port = "8080";
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            Console.WriteLine($"Listening on port {port}...");
            return listener;
        }

        private static TelegramBotClient ConfigureBot(string apiToken, CancellationTokenSource cancellationToken)
        {
            var bot = new TelegramBotClient(apiToken);
            var updateProcessor = new UpdateMessageProcessor(
               new UserDataStorage(UserDataStorage.GetDefaultPath()),
               new MessageSender(),
               new CommandProcessor(new CommandParser()));
            var handler = new BotHandler(bot, updateProcessor);
            Console.WriteLine("Starting bot...");
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cancellationToken.Token);
            Console.WriteLine("Bot is running.");
            return bot;
        }
        
        private static async Task HandleHttpRequestsAsync(HttpListener listener, CancellationTokenSource cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    Console.WriteLine($"Received HTTP request: {context.Request.RawUrl}");
                    context.Response.StatusCode = 200;
                    await using var writer = new StreamWriter(context.Response.OutputStream);
                    await writer.WriteAsync("OK");
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    Console.WriteLine("Listener stopped.");
                }
            }
        }

        private static async Task HandleShutdownAsync(CancellationTokenSource cts)
        {
            EventHandler processExitHandler = (_, _) => CancelTokenSafely(cts);
            ConsoleCancelEventHandler cancelKeyPressHandler = (_, e) =>
            {
                e.Cancel = true;
                CancelTokenSafely(cts);
            };

            AppDomain.CurrentDomain.ProcessExit += processExitHandler;
            Console.CancelKeyPress += cancelKeyPressHandler;
            try
            {
                await Task.Delay(-1, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                Console.CancelKeyPress -= cancelKeyPressHandler;
            }
        }

        private static void StopServices(HttpListener listener, CancellationTokenSource cts)
        {
            Console.WriteLine("Stopping services...");
            CancelTokenSafely(cts);
            listener.Stop();
        }

        private static void CancelTokenSafely(CancellationTokenSource cts)
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}
