namespace JNJServices.API.Hubs
{
    public class ConnectionMapping : IConnectionMapping
    {
        private readonly Dictionary<string, HashSet<string>> _connections =
        new Dictionary<string, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Values.Sum(hashSet => hashSet.Count);
            }
        }

        public void Add(string key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(string key)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(key, out var connections))
                {
                    return connections.ToList(); // Return a copy to avoid unintended modifications
                }
            }
            return Enumerable.Empty<string>();
        }

        public void Remove(string key, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(key, out var connections)) return;

                lock (connections)
                {
                    if (connections.Count == 1 && connections.Contains(connectionId))
                    {
                        _connections.Remove(key); // Remove entire key if last connection
                    }
                    else
                    {
                        connections.Remove(connectionId);
                    }
                }
            }
        }

        public IEnumerable<string> Keys => _connections.Keys;

        public bool IsConnectionActive(string key, string connectionId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(key, out var connections) && connections.Contains(connectionId);
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> ConnectionKeys
        {
            get
            {
                lock (_connections)
                {
                    return _connections.ToDictionary(pair => pair.Key, pair => (IReadOnlyCollection<string>)pair.Value);
                }
            }
        }

        public IEnumerable<string> GetAllKeys()
        {
            lock (_connections)
            {
                return _connections.Keys.ToList(); // Convert to a list to avoid threading issues
            }
        }


    }
}
