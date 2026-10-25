using System.Collections.Concurrent;


namespace UrlShortner.Stores;

public class VisitStore : IVisitStore
{
    private readonly ConcurrentDictionary<string, int> _visitStore = new();


    public IReadOnlyCollection<KeyValuePair<string, int>> Drain()
    {
        List<KeyValuePair<string, int>> result = new();

        foreach (var pair in _visitStore)
        {
            if (_visitStore.TryRemove(pair.Key, out var removedValue))
                result.Add(new KeyValuePair<string, int>(pair.Key, removedValue));
        }

        return result;
    }

    public void AddVisitCount(string url) 
        => _visitStore.AddOrUpdate(url, 1, (key, currentCount) => currentCount + 1);
}