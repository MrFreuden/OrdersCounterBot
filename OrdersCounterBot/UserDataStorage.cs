using System.Text.Json;

namespace OrdersCounterBot
{
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
}
