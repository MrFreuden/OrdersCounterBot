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

        public Response ProcessCommand(UserService userService, CommandContext commandContext)
        {
            var command = _parser.Parse(commandContext.Text);
            return command.Invoke(userService, commandContext.UserId, commandContext.ChatId);
        }
    }
}