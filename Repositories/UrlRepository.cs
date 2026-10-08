using System.Collections.Concurrent;
using UrlShortner.Models;


namespace UrlShortner.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, Url> _urls = new();


    public bool TryAdd(Url url) => _urls.TryAdd(url.GeneratedUrl, url);

    public bool Remove(string shortUrl) => _urls.Remove(shortUrl, out var _);

    public IReadOnlyCollection<Url> GetAll() => (IReadOnlyCollection<Url>)_urls.Values;

    public Url? Get(string shortUrl)
    {
        _urls.TryGetValue(shortUrl, out var url);

        return url;
    }
}