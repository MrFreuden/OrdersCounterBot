using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrdersCounterBot
{
    public class BotHandler
    {
        private readonly IUpdateProcessor _updateProcessor;

        public BotHandler(IUpdateProcessor updateProcessor)
        {
            _updateProcessor = updateProcessor;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                 await _updateProcessor.Process(client, update); 
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000, token);
        }
    }

    public interface IUpdateProcessor
    {
        Task Process(ITelegramBotClient client, Update update);
    }

    public class ProccessMessageUpdate : IUpdateProcessor
    {
        private readonly UserService _userService;
        private readonly UserDataStorage _dataStorage;
        private readonly MessageSender _messageSender;
        private readonly CommandProcessor _commandProcessor;

        public ProccessMessageUpdate(UserDataStorage dataStorage, MessageSender messageSender, CommandProcessor commandProcessor)
        {
            _dataStorage = dataStorage;
            _messageSender = messageSender;
            _commandProcessor = commandProcessor;
            _userService = _dataStorage.LoadData();
        }

        public async Task Process(ITelegramBotClient client, Update update)
        {
            var userId = update.Message.From.Id;
            var response = _commandProcessor.ProcessCommand(update.Message.Text!, _userService, userId);

            await _messageSender.SendResponseAsync(client, update.Message.Chat.Id, response);
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

        public Response ProcessCommand(string messageText, UserService userService, long userId)
        {
            var command = _parser.Parse(messageText);
            return command.Invoke(userService, userId);
        }
    }
}
