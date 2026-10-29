using UrlShortner.DTOs;


namespace UrlShortner.Stores;

public interface IInFlightStore
{
    Lazy<Task<string>> GetOrAddUrl(string url, Func<Task<string>> value);
    Lazy<Task<IReadOnlyCollection<GetUrlDto>>> GetOrAddUrlList(string url, Func<Task<IReadOnlyCollection<GetUrlDto>>> value);
    void TryRemove(string url);
}