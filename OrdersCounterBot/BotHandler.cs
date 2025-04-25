using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrdersCounterBot
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

    public interface IUpdateProcessor
    {
        Task ProcessMessage(ITelegramBotClient client, Message update);
    }

    public class UpdateMessageProcessor : IUpdateProcessor
    {
        private readonly UserService _userService;
        private readonly UserDataStorage _dataStorage;
        private readonly MessageSender _messageSender;
        private readonly CommandProcessor _commandProcessor;

        public UpdateMessageProcessor(UserDataStorage dataStorage, MessageSender messageSender, CommandProcessor commandProcessor)
        {
            _dataStorage = dataStorage;
            _messageSender = messageSender;
            _commandProcessor = commandProcessor;
            _userService = _dataStorage.LoadData();
        }

        public async Task ProcessMessage(ITelegramBotClient client, Message msg)
        {
            var userId = msg.From.Id;
            if (msg.Type == MessageType.MigratedFromGroup || msg.Type == MessageType.MigratedToSupergroup)
            {
                var old = msg.MigrateFromChatId;
                var idNew = msg.MigrateToChatId;
                _userService.ChangeChatId(userId, (long)old, (long)idNew);
                await _dataStorage.SaveDataAsync(_userService);
                return;
            }
            
            var chatId = msg.Chat.Id;
            var response = _commandProcessor.ProcessCommand(msg.Text!, _userService, userId, chatId);

            if (!string.IsNullOrEmpty(response.Text))
            {
                await _messageSender.SendResponseAsync(client, msg.Chat.Id, response);
            }
            await _dataStorage.SaveDataAsync(_userService);
        }
    }

    public class MessageSender
    {
        public async Task SendResponseAsync(ITelegramBotClient client, ChatId chatId, Response response)
        {
            await client.SendTextMessageAsync(chatId, response.Text);
        }
    }

    public class CommandProcessor
    {
        private readonly CommandParser _parser;

        public CommandProcessor(CommandParser parser)
        {
            _parser = parser;
        }

        public Response ProcessCommand(string messageText, UserService userService, long userId, long chatId)
        {
            var command = _parser.Parse(messageText);
            return command.Invoke(userService, userId, chatId);
        }
    }
}
