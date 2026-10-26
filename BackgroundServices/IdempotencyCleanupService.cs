using Microsoft.Extensions.Options;
using UrlShortner.Settings;
using UrlShortner.Stores;


namespace UrlShortner.BackgroundServices;

public class IdempotencyCleanupService : BackgroundService
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IOptions<IdempotencySettings> _options;


    public IdempotencyCleanupService(IIdempotencyStore idempotencyStore, IOptions<IdempotencySettings> options)
    {
        _idempotencyStore = idempotencyStore;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(_options.Value.CleanupIntervalMinutes), stoppingToken);

            await _idempotencyStore.RemoveExpiredAsync(TimeSpan.FromHours(_options.Value.TtlHours), TimeSpan.FromMinutes(_options.Value.StaleProcessingMinutes));
        }
    }
}