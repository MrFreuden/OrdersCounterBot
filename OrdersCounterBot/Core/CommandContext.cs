namespace OrdersCounterBot.Core
{
    public class CommandContext
    {
        public readonly long UserId;
        public readonly long ChatId;
        public readonly string Text;

        public CommandContext(long userId, long chatId, string text)
        {
            UserId = userId;
            ChatId = chatId;
            Text = text;
        }
    }
}
