namespace JNJServices.API.Hubs
{
    public interface IConnectionMapping
    {
        int Count { get; }
        void Add(string key, string connectionId);
        IEnumerable<string> GetConnections(string key);
        void Remove(string key, string connectionId);
        IEnumerable<string> Keys { get; }
        bool IsConnectionActive(string key, string connectionId);
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> ConnectionKeys { get; }
        IEnumerable<string> GetAllKeys();
    }
}
