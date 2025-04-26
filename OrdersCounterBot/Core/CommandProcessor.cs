using OrdersCounterBot.Services;

namespace OrdersCounterBot.Core
{
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
