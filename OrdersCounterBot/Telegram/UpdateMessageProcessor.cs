using OrdersCounterBot.Core;
using OrdersCounterBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrdersCounterBot.Telegram
{
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
}
