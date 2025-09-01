using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrdersCounterBot.Core
{
    public interface IUpdateProcessor
    {
        Task ProcessChatMember(ChatMemberUpdated myChatMember);
        Task ProcessMessage(ITelegramBotClient client, Message update);
    }
}
