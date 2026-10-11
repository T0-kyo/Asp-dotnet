using System.Collections.Concurrent;


namespace UrlShortner.Stores;

public class VisitStore : IVisitStore
{
    private readonly ConcurrentDictionary<string, int> _visitStore = new();


    public IReadOnlyCollection<KeyValuePair<string, int>> Drain()
    {
        IReadOnlyCollection<KeyValuePair<string, int>> results = [.. _visitStore];

        _visitStore.Clear();
        return results;
    }

    public void AddVisitCount(string url)
    {
        if (_visitStore.TryGetValue(url, out var count))
            Interlocked.Increment(ref count);
        
        _visitStore.TryAdd(url, 1);
    }
}