using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using UrlShortner.Models.Types;
using UrlShortner.Settings;
using UrlShortner.Models;


namespace UrlShortner.Stores;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord> _store = new();
    private readonly IOptions<IdempotencySettings> _options;


    public IdempotencyStore(IOptions<IdempotencySettings> options)
    {
        _options = options;
    }

    public async Task<bool> TryAddAsync(string key, IdempotencyRecord record) => _store.TryAdd(key, record);

    public async Task UpdateAsync(string key, IdempotencyRecord record) => _store[key] = record;

    public async Task<bool> TryUpdateStaleAsync(string key, IdempotencyRecord record)
    {
        if(!_store.TryGetValue(key, out var existingRecord))
            return false;

        if (existingRecord.Status == IdempotencyStatus.Pending && 
            DateTime.UtcNow - existingRecord.CreatedAt >= TimeSpan.FromMinutes(_options.Value.StaleProcessingMinutes))
        {
            return _store.TryUpdate(key, record, existingRecord);
        }

        return false;
    }

    public async Task<IdempotencyRecord?> GetRecordAsync(string key)
    {
        if (_store.TryGetValue(key, out var result))
            return result;

        return null;
    }

    public async Task RemoveExpiredAsync(TimeSpan ttl, TimeSpan staleProcessing)
    {
        foreach (var record in _store)
        {
            if (record.Value.Status == IdempotencyStatus.Pending && DateTime.UtcNow - record.Value.CreatedAt >= staleProcessing)
                _store.Remove(record.Key, out var _);

            if (record.Value.Status == IdempotencyStatus.Completed && DateTime.UtcNow - record.Value.CreatedAt >= ttl)
                _store.Remove(record.Key, out var _);
        }
    }
}