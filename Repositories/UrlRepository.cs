using System.Collections.Concurrent;
using UrlShortner.Models;


namespace UrlShortner.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, Url> _urls = new();


    public async Task<bool> TryAddAsync(Url url) => _urls.TryAdd(url.GeneratedUrl, url);

    public async Task<bool> RemoveAsync(string shortUrl) => _urls.Remove(shortUrl, out var _);

    public async Task<IReadOnlyCollection<Url>> GetAllAsync() => (IReadOnlyCollection<Url>)_urls.Values;

    public async Task<Url?> GetAsync(string shortUrl)
    {
        _urls.TryGetValue(shortUrl, out var url);

        return url;
    }

    public async Task RemoveExpiredAsync()
    {
        var expiredUrls = _urls.Where(u => u.Value.ExpiresAt <= DateTime.UtcNow);

        foreach (var url in expiredUrls)
            _urls.Remove(url.Key, out var _);
    }

    public async Task IncrementVisitCountAsync(IReadOnlyCollection<KeyValuePair<string, int>> pendingVisits)
    {
        foreach (var url in pendingVisits)
            _urls[url.Key].IncreaseVisitCount(url.Value);
    }
}