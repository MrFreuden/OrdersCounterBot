namespace OrdersCounterBot
{
    [Serializable]
    public class OldUserService
    {
        private readonly object _lock = new();

        public Dictionary<long, List<int>> _userLists { get; set; }

        public OldUserService()
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

    [Serializable]
    public class UserService
    {
        private readonly object _lock = new();

        public Dictionary<long, Dictionary<long, int>> _usersSums { get; set; }

        public UserService()
        {
            _usersSums = new();
        }

        public UserService(OldUserService oldUserService)
        {
            _usersSums = new();
            var pairs = oldUserService._userLists.Select(x => (x.Key, x.Value.Sum()));
            foreach (var pair in pairs)
            {
                AddNewUser(pair.Key, 0);
                AddData(pair.Key, 0, pair.Item2);
            }
        }

        public bool IsUserExists(long userId)
        {
            lock (_lock)
            {
                return _usersSums.ContainsKey(userId);
            }
        }

        public void AddNewUser(long userId, long chatId)
        {
            lock (_lock)
            {
                if (!_usersSums.ContainsKey(userId))
                {
                    _usersSums.Add(userId, new Dictionary<long, int> { { chatId, 0 } });
                }
            }
        }

        public void AddNewChatId(long userId, long chatId)
        {
            lock (_lock)
            {
                if (!_usersSums.ContainsKey(userId))
                {
                    _usersSums[userId].Add(chatId, 0);
                }
            }
        }

        public void ChangeChatId(long userId, long chatIdOld, long chatIdNew)
        {
            lock (_lock)
            {
                if (IsIdsValid(userId, chatIdOld))
                {
                    AddNewChatId(userId, chatIdNew);
                    _usersSums[userId][chatIdNew] = _usersSums[userId][chatIdOld];
                    _usersSums[userId].Remove(chatIdOld);
                }
            }
        }

        public void AddData(long userId, long chatId, int value)
        {
            lock (_lock)
            {
                if (IsIdsValid(userId, chatId))
                {
                    _usersSums[userId][chatId] += value;
                }
            }
        }

        public void ClearData(long userId, long chatId)
        {
            lock (_lock)
            {
                if (IsIdsValid(userId, chatId))
                    _usersSums[userId][chatId] = 0;
            }
        }

        public int GetSum(long userId, long chatId)
        {
            lock (_lock)
            {
                if (IsIdsValid(userId, chatId))
                    return _usersSums[userId][chatId];
                return 0;
            }
        }

        private bool IsIdsValid(long userId, long chatId)
        {
            return _usersSums.ContainsKey(userId) && _usersSums[userId].ContainsKey(chatId);
        }
    }
}
