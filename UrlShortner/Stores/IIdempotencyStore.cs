using UrlShortner.Models;


namespace UrlShortner.Stores;

public interface IIdempotencyStore
{
    Task<bool> TryAddAsync(string key, IdempotencyRecord record);
    Task UpdateAsync(string key, IdempotencyRecord record);
    Task<bool> TryUpdateStaleAsync(string key, IdempotencyRecord record);
    Task<IdempotencyRecord?> GetRecordAsync(string key);
    Task RemoveExpiredAsync(TimeSpan tll, TimeSpan staleProcessing);
}