using Microsoft.Extensions.Options;
using UrlShortner.Settings;
using UrlShortner.Stores;


namespace UrlShortner.BackgroundServices;

public class IdempotencyCleanupService : BackgroundService
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IOptions<IdempotencySettings> _options;
    private readonly ILogger<IdempotencyCleanupService> _logger;


    public IdempotencyCleanupService(IIdempotencyStore idempotencyStore, IOptions<IdempotencySettings> options, ILogger<IdempotencyCleanupService> logger)
    {
        _idempotencyStore = idempotencyStore;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.Value.CleanupIntervalMinutes), stoppingToken);

                await _idempotencyStore.RemoveExpiredAsync(TimeSpan.FromHours(_options.Value.TtlHours), TimeSpan.FromMinutes(_options.Value.StaleProcessingMinutes));   
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error from idempotency cleanup service.");
            }
        }
    }
}