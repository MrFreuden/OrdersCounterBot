using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrdersCounterBot
{
    public class BotHandler
    {
        private readonly UserService _userService;
        private readonly CommandParser _parser;
        private readonly UserDataStorage _dataStorage;

        public BotHandler(UserDataStorage dataStorage, CommandParser parser)
        {
            _dataStorage = dataStorage;
            _parser = parser;
            _userService = _dataStorage.LoadData();
        }

        private void PrintList(IReadOnlyList<int> list)
        {
            Console.Clear();
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Console.WriteLine("HANDLE");
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var userId = update.Message.From.Id;
                var command = _parser.Parse(update.Message.Text);

                var response = command.Invoke(_userService, userId);
                await client.SendTextMessageAsync(update.Message.Chat.Id, response.Text);
                PrintList(_userService.GetList(userId));
                await _dataStorage.SaveDataAsync(_userService);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000, token);
        }
    }
}
