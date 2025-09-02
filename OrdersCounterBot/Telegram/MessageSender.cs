using OrdersCounterBot.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrdersCounterBot.Telegram
{
    public class MessageSender
    {
        public async Task SendResponseAsync(ITelegramBotClient client, ChatId chatId, Response response)
        {
            await client.SendMessage(chatId, response.Text);
        }
    }
}
