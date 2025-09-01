using OrdersCounterBot.Configuration;
using OrdersCounterBot.Core;
using OrdersCounterBot.Services;
using OrdersCounterBot.Telegram;
using System.Net;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrdersCounterBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            EnvLoader.Load();

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Console.WriteLine($"Starting application (env: {env})");

            try
            {
                var apiToken = EnvLoader.GetApiToken()
                    ?? throw new InvalidOperationException("API_TOKEN not set");


                var bot = new TelegramBotClient(apiToken);

                var updateProcessor = new UpdateMessageProcessor(
                   new UserDataStorage(EnvLoader.GetDataPath()),
                   new MessageSender(),
                   new CommandProcessor(new CommandParser()));

                var handler = new BotHandler(bot, updateProcessor);

                if (env == "Development")
                {
                    await RunPolling(bot, handler);
                }
                else
                {
                    var webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL")
                        ?? throw new InvalidOperationException("WEBHOOK_URL not set");
                    await RunWebhook(bot, handler, webhookUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex}");
            }
            finally
            {
                Console.WriteLine("Application stopped.");
            }
        }

        private static async Task RunPolling(ITelegramBotClient bot, BotHandler handler)
        {
            using var cts = new CancellationTokenSource();
            Console.WriteLine("Starting in polling mode...");
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cts.Token);
            Console.WriteLine("Bot is running (polling). Press Ctrl+C to exit.");
            await WaitForShutdown(cts);
            Console.WriteLine("Polling stopped.");
        }

        private static async Task RunWebhook(ITelegramBotClient bot, BotHandler handler, string webhookUrl)
        {
            using var cts = new CancellationTokenSource();
            Console.WriteLine($"Setting webhook to {webhookUrl}");

            await bot.SetWebhookAsync(webhookUrl);

            var listener = new HttpListener();
            var prefix = EnvLoader.GetListenerPrefix();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine($"Listening HTTP on {prefix}");

            var httpTask = HandleHttp(listener, bot, handler, cts.Token);
            var shutdownTask = WaitForShutdown(cts);
            await Task.WhenAny(httpTask, shutdownTask);

            try { listener.Stop(); } catch { /* ignore */ }
            Console.WriteLine("Webhook stopped.");
        }

        private static async Task HandleHttp(HttpListener listener, ITelegramBotClient bot, BotHandler handler, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext? ctx = null;
                try
                {
                    ctx = await listener.GetContextAsync();
                    if (ctx.Request.HttpMethod != "POST")
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.Close();
                        continue;
                    }

                    using var reader = new StreamReader(ctx.Request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    Console.WriteLine($"Received update JSON: {body}");

                    Update? update = JsonSerializer.Deserialize<Update>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (update != null)
                    {
                        await handler.HandleUpdateAsync(bot, update, token);
                    }
                    else
                    {
                        Console.WriteLine("Invalid update received");
                    }

                    ctx.Response.StatusCode = 200;
                    ctx.Response.Close();
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    Console.WriteLine("HttpListener stopped");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unhandled error in HTTP handler: {ex}");
                    if (ctx != null)
                    {
                        try
                        {
                            ctx.Response.StatusCode = 500;
                            ctx.Response.Close();
                        }
                        catch { /* ignore */ }
                    }
                }
            }
        }

        private static async Task WaitForShutdown(CancellationTokenSource cts)
        {
            EventHandler onExit = (_, _) => CancelTokenSafely(cts);
            ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; CancelTokenSafely(cts); };

            AppDomain.CurrentDomain.ProcessExit += onExit;
            Console.CancelKeyPress += onCancel;
            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException) { }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= onExit;
                Console.CancelKeyPress -= onCancel;
            }
        }

        private static void CancelTokenSafely(CancellationTokenSource cts)
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}