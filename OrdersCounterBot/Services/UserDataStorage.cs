using Newtonsoft.Json;

namespace OrdersCounterBot.Services
{
    public class UserDataStorage
    {
        private readonly string _path;
        private readonly object _lock = new();

        public UserDataStorage(string path)
        {
            _path = path;
        }

        public UserService LoadData()
        {
            lock (_lock)
            {
                if (!File.Exists(_path))
                {
                    return new UserService();
                }
                try
                {
                    var jsonString = File.ReadAllText(_path);
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
    }
}
