namespace OrdersCounterBot
{
    public class CommandParser
    {
        public Func<UserService, long, Response> Parse(string text)
        {
            var splited = text.Replace(" ", "").ToLower();
            if (splited.StartsWith("/start"))
            {
                return (list, id) => { list.AddNewUser(id); return new Response("Добро пожаловать"); };
            }
            else if (splited.StartsWith("end"))
            {
                return (list, id) => { list.ClearData(id); return new Response("Конец дня"); };
            }
            else if (Int32.TryParse(splited, out int value))
            {
                return (list, id) => { list.AddData(id, value); return new Response(list.GetSum(id).ToString()); };
            }
            else if (splited[0] == '=')
            {
                return (list, id) => new Response(list.GetSum(id).ToString());
            }

            return (list, id) => new Response("Неизвестная команда");
        }
    }
}
