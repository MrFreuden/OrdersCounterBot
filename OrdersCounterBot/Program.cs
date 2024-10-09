using System.Net;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrdersCounterBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        private static async void Run()
        {
            var listener = new HttpListener();
            var port = "8080";
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();

            Console.WriteLine($"Listening on port {port}...");

            var apiToken = GetApiToken();
            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(apiToken);
            var handler = new BotHandler(GetUserDataStorage(), new CommandParser());
            bot.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, cancellationToken: cts.Token);
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel();
            listener.Stop();
        }
        private static UserDataStorage GetUserDataStorage()
        {
            string? filePath = null;
            if (Environment.GetEnvironmentVariable("SERVER_ENV") == "true")
            {
                filePath = "/secrets/data.json";
            }
            else
            {
                filePath = "data.json";
            }
            return new UserDataStorage(filePath);
        }
        private static string GetApiToken()
        {
            DotNetEnv.Env.Load();
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
            if (apiToken == null) throw new ArgumentNullException(apiToken);
            return apiToken;
        }
    }

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

    public class UserDataStorage
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
        private readonly object _lock = new();

        public UserDataStorage(string path)
        {
            _path = path;
        }

        public UserService LoadData()
        {
            lock (_lock)
            {
                if (!System.IO.File.Exists(_path))
                {
                    return new UserService();
                }
                try
                {
                    string jsonString = System.IO.File.ReadAllText(_path);
                    if (jsonString != null) Console.WriteLine("Загрузка успешна");
                    return JsonSerializer.Deserialize<UserService>(jsonString) ?? new UserService();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    return new UserService();
                }
            }
        }

        private void SaveData(UserService service)
        {
            lock (_lock)
            {
                try
                {
                    string jsonString = JsonSerializer.Serialize(service);
                    System.IO.File.WriteAllText(_path, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                }
            }
        }

        public async Task SaveDataAsync(UserService service)
        {
            Console.WriteLine("Начало сохранения");
            await Task.Run(() => SaveData(service));
            Console.WriteLine("Конец сохранения");
        }
    }

    [Serializable]
    public class UserService
    {
        private readonly object _lock = new();

        public Dictionary<long, List<int>> _userLists { get; set; }

        public UserService()
        {
            _userLists = new();
        }

        public bool UserExists(long userId)
        {
            lock (_lock)
            {
                return _userLists.ContainsKey(userId);
            }
        }

        public void AddNewUser(long userId)
        {
            lock (_lock)
            {
                if (!_userLists.ContainsKey(userId))
                {
                    _userLists.Add(userId, new List<int>());
                }
            }
        }

        public void AddData(long userId, int value)
        {
            lock (_lock)
            {
                _userLists[userId].Add(value);
            }
        }

        public void ClearData(long userId)
        {
            lock (_lock)
            {
                if (_userLists.TryGetValue(userId, out List<int>? value))
                    value.Clear();
            }
        }

        public void RemoveLast(long userId)
        {
            lock (_lock)
            {
                _userLists[userId].RemoveAt(_userLists[userId].Count - 1);
            }
        }

        public int GetSum(long userId)
        {
            lock (_lock)
            {
                return _userLists.TryGetValue(userId, out var list) ? list.Sum() : 0;
            }
        }

        public IReadOnlyList<int> GetList(long userId)
        {
            lock (_lock)
            {
                return _userLists.TryGetValue(userId, out var list) ? list.AsReadOnly() : new List<int>();
            }
        }
    }

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

    public class Response
    {
        public readonly string Text;
        public Response(string text)
        {
            Text = text;
        }
    }
}
