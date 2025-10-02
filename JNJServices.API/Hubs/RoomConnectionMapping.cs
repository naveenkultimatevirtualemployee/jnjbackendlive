namespace JNJServices.API.Hubs
{
    public class RoomConnectionMapping : IRoomConnectionMapping
    {
        private readonly Dictionary<string, HashSet<string>> _roomConnections = new();

        public void RemoveUserFromAllRooms(string userId, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            lock (_roomConnections)
            {
                var roomsToRemove = new List<string>();

                foreach (var (room, users) in _roomConnections)
                {
                    if (users.Remove(userId) && users.Count == 0)
                    {
                        roomsToRemove.Add(room);
                    }
                }

                foreach (var room in roomsToRemove)
                {
                    _roomConnections.Remove(room);
                }
            }
        }

        public void AddToRoom(string roomName, string userId)
        {
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(userId))
                return;

            lock (_roomConnections)
            {
                if (!_roomConnections.TryGetValue(roomName, out var users))
                {
                    users = new HashSet<string>();
                    _roomConnections[roomName] = users;
                }

                users.Add(userId);
            }
        }

        public void RemoveFromRoom(string roomName, string userId)
        {
            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(userId))
                return;

            lock (_roomConnections)
            {
                if (!_roomConnections.TryGetValue(roomName, out var users))
                    return;

                users.Remove(userId);

                if (users.Count == 0)
                {
                    _roomConnections.Remove(roomName);
                }
            }
        }

        public int GetConnectionCount(string roomName)
        {
            lock (_roomConnections)
            {
                return _roomConnections.TryGetValue(roomName, out var users)
                    ? users.Count
                    : 0;
            }
        }

        public List<string> GetUsers(string roomName)
        {
            lock (_roomConnections)
            {
                return _roomConnections.TryGetValue(roomName, out var users)
                    ? new List<string>(users)
                    : new List<string>();
            }
        }

        public string GetAllUserIdsInRoomCommaSeparated(string roomName)
        {
            var users = GetUsers(roomName);
            return users.Count > 0 ? string.Join(",", users) : string.Empty;
        }

        public Dictionary<string, List<string>> GetAllRoomsWithUsers()
        {
            lock (_roomConnections)
            {
                return _roomConnections.ToDictionary(
                    pair => pair.Key,
                    pair => new List<string>(pair.Value)
                );
            }
        }
    }
}
