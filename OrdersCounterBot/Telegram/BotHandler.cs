using OrdersCounterBot.Core;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace OrdersCounterBot.Telegram
{
    public class BotHandler : IUpdateHandler
    {
        private readonly IUpdateProcessor _updateProcessor;
        private readonly ITelegramBotClient _bot;
        public BotHandler(ITelegramBotClient botClient, IUpdateProcessor updateProcessor)
        {
            _bot = botClient;
            _updateProcessor = updateProcessor;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => _updateProcessor.ProcessMessage(_bot, message),
                //{ EditedMessage: { } message } => OnMessage(message),
                //{ CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
                //{ InlineQuery: { } inlineQuery } => OnInlineQuery(inlineQuery),
                //{ ChosenInlineResult: { } chosenInlineResult } => OnChosenInlineResult(chosenInlineResult),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            return Task.CompletedTask;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000, cancellationToken);
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}