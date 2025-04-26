using OrdersCounterBot.Services;

namespace OrdersCounterBot.Core
{
    public class CommandParser
    {
        public Func<UserService, long, long, Response> Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (list, userId, chatId) => { return new Response(""); };
            }
            var splited = text.Replace(" ", "").ToLower();

            if (splited.StartsWith("/start"))
            {
                return (list, userId, chatId) => { list.AddNewUser(userId, chatId); return new Response("Добро пожаловать"); };
            }
            else if (splited.StartsWith("end"))
            {
                return (list, userId, chatId) => { list.ClearData(userId, chatId); return new Response("Конец дня"); };
            }
            else if (int.TryParse(splited, out int value))
            {
                return (list, userId, chatId) => { list.AddData(userId, chatId, value); return new Response(list.GetSum(userId, chatId).ToString()); };
            }
            else if (splited[0] == '=')
            {
                return (list, userId, chatId) => new Response(list.GetSum(userId, chatId).ToString());
            }

            return (list, userId, chatId) => new Response("Неизвестная команда");
        }
    }
}