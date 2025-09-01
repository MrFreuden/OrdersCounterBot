namespace OrdersCounterBot.Services
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
                if (_usersSums.TryGetValue(userId, out var chatIds))
                {
                    if (chatIds.ContainsKey(chatId))
                    {
                        chatIds[chatId] += value;
                    }
                    else
                    {
                        chatIds[chatId] = value;
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
                {
                    var val = _usersSums[userId][chatId];
                    if (val == 0)
                    {
                        ClearData(userId, chatId);
                    }
                    return val;
                }    
                return 0;
            }
        }

        private bool IsIdsValid(long userId, long chatId)
        {
            return _usersSums.ContainsKey(userId) && _usersSums[userId].ContainsKey(chatId);
        }
    }
}
