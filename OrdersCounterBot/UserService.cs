namespace OrdersCounterBot
{
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
                if (_userLists.ContainsKey(userId))
                {
                    _userLists[userId].Add(value);
                }
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
}
