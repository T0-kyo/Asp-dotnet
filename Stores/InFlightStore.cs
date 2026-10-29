using System.Collections.Concurrent;
using UrlShortner.DTOs;


namespace UrlShortner.Stores;

public class InFlightStore : IInFlightStore
{
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _inFlight = new();
    private readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyCollection<GetUrlDto>>>> _inFlightList = new();


    public Lazy<Task<string>> GetOrAddUrl(string url, Func<Task<string>> value)
    {
        return _inFlight.GetOrAdd(url, _ => new Lazy<Task<string>>(value));
    }

    public Lazy<Task<IReadOnlyCollection<GetUrlDto>>> GetOrAddUrlList(string url, Func<Task<IReadOnlyCollection<GetUrlDto>>> value)
    {
        return _inFlightList.GetOrAdd(url, _ => new Lazy<Task<IReadOnlyCollection<GetUrlDto>>>(value));
    }

    public void TryRemove(string url)
    {
        _inFlight.TryRemove(url, out _);
        _inFlightList.TryRemove(url, out _);
    }
}