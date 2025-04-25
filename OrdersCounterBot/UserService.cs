namespace OrdersCounterBot
{
    [Serializable]
    public class UserService
    {
        private readonly object _lock = new();

        public Dictionary<long, Dictionary<long, int>> _usersSums { get; set; }

        public UserService()
        {
            _usersSums = new();
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
                    var chats = _usersSums[userId];

                    if (!chats.ContainsKey(chatId))
                    {
                        chats[chatId] = 0;
                    }
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
                if (_usersSums.TryGetValue(userId, out var dic))
                {
                    if (dic.ContainsKey(chatId))
                    {
                        _usersSums[userId][chatId] += value;
                    }
                    else
                    {
                        _usersSums[userId][chatId] = value;
                    }
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
