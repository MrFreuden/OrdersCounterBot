using Newtonsoft.Json;

namespace OrdersCounterBot
{
    public class UserDataStorage
    {
        private readonly string _path;
        private readonly object _lock = new();

        public UserDataStorage(string path)
        {
            _path = path;
        }

        public OldUserService LoadData()
        {
            lock (_lock)
            {
                if (!File.Exists(_path))
                {
                    return new OldUserService();
                }
                try
                {
                    var jsonString = File.ReadAllText(_path);
                    if (jsonString != null) Console.WriteLine("Загрузка успешна");
                    return JsonConvert.DeserializeObject<OldUserService>(jsonString) ?? new OldUserService();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    return new OldUserService();
                }
            }
        }

        public UserService LoadData2()
        {
            lock (_lock)
            {
                if (!File.Exists(_path))
                {
                    return new UserService();
                }
                try
                {
                    var jsonString = File.ReadAllText(DefaultPaths.LocalDataPath);
                    if (jsonString != null) Console.WriteLine("Загрузка успешна");
                    return JsonConvert.DeserializeObject<UserService>(jsonString) ?? new UserService();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    return new UserService();
                }
            }
        }

        public async Task SaveDataAsync(UserService service)
        {
            Console.WriteLine("Начало сохранения");
            await Task.Run(() => SaveData(service));
            Console.WriteLine("Конец сохранения");
        }

        private void SaveData(UserService service)
        {
            lock (_lock)
            {
                try
                {
                    var jsonString = JsonConvert.SerializeObject(service);
                    File.WriteAllText(_path, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                }
            }
        }

        public static string GetDefaultPath()
        {
            var directory = "/OrdersCounterBot/docker_data";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory + "/data.json";
        }
    }

    public static class DefaultPaths
    {
        public const string ServerDataPath = "/secrets/data.json";
        public const string LocalDataPath = "data.json";
    }
}
