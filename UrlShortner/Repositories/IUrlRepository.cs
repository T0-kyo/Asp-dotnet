using UrlShortner.Models;


namespace UrlShortner.Repositories;

public interface IUrlRepository
{
    Task<bool> TryAddAsync(Url url);
    Task<bool> RemoveAsync(string shortUrl);
    Task RemoveExpiredAsync();
    Task<Url?> GetAsync(string shortUrl);
    Task<IReadOnlyCollection<Url>> GetAllAsync();
    Task IncrementVisitCountAsync(IReadOnlyCollection<KeyValuePair<string, int>> pendingVisits);
}