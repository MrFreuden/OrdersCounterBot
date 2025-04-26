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
            if (msg.From == null) return;

            if (await HandleMigrationAsync(msg)) return;

            if (string.IsNullOrEmpty(msg.Text)) return;

            var commandContext = GetContext(msg);

            var response = _commandProcessor.ProcessCommand(_userService, commandContext);

            await _messageSender.SendResponseAsync(client, msg.Chat.Id, response);

            await _dataStorage.SaveDataAsync(_userService);
        }

        private async Task<bool> HandleMigrationAsync(Message msg)
        {
            if (msg.Type == MessageType.MigratedFromGroup || msg.Type == MessageType.MigratedToSupergroup)
            {
                var old = msg.MigrateFromChatId;
                var idNew = msg.MigrateToChatId;
                _userService.ChangeChatId(msg.From.Id, (long)old, (long)idNew);
                await _dataStorage.SaveDataAsync(_userService);
                return true;
            }
            return false;
        }

        private CommandContext GetContext(Message msg)
        {
            return new CommandContext(msg.From.Id, msg.Chat.Id, msg.Text);
        }
    }
}
