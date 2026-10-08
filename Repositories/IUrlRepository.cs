using UrlShortner.Models;


namespace UrlShortner.Repositories;

public interface IUrlRepository
{
    bool TryAdd(Url url);
    bool Remove(string shortUrl);
    Url? Get(string shortUrl);
    IReadOnlyCollection<Url>? GetAll();
}