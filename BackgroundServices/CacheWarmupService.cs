using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UrlShortner.Services;
using UrlShortner.Settings;


namespace UrlShortner.BackgroundServices;

public class CacheWarmupService : BackgroundService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CacheWarmupService> _logger;
    private readonly IOptions<CacheSettings> _cacheOptions;


    public CacheWarmupService(IMemoryCache cache, IServiceScopeFactory scopeFactory, ILogger<CacheWarmupService> logger, IOptions<CacheSettings> cacheOptions)
    {
        _scopeFactory = scopeFactory;
        _cacheOptions = cacheOptions;
        _logger = logger;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string key = "urls:v1:creator=:category=:page=1:pageSize=10";

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var urlService = scope.ServiceProvider.GetRequiredService<IUrlService>();

                var urls = await urlService.GetAllUrlsAsync(null, null, null, page: 1, pageSize: 10);
                var ttl = TimeSpan.FromMinutes(_cacheOptions.Value.GetAllTtlMinutes);
                var jitter = TimeSpan.FromSeconds(Random.Shared.Next(_cacheOptions.Value.GetAllJitterMinValue, _cacheOptions.Value.GetAllJitterMaxValue));
        
                _cache.Set(key, urls, ttl + jitter);
    
                _logger.LogInformation("Cache Warmup for Url list succeeded!");
            }
        }
        catch (Exception)
        {
            _logger.LogWarning("Cache Warmup failed!");
        }
    }
}