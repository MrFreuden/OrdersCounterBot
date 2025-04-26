using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrdersCounterBot.Core
{
    public interface IUpdateProcessor
    {
        Task ProcessMessage(ITelegramBotClient client, Message update);
    }
}
