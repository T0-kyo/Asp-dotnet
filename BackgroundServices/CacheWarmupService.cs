using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UrlShortner.DTOs;
using UrlShortner.Repositories;
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
        string key = $"urls:v1:creator=:category=:page=1:pageSize={_cacheOptions.Value.GetUrlLimit}";

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var urlRepository = scope.ServiceProvider.GetRequiredService<IUrlRepository>();

                var urls = await urlRepository.GetAllAsync();

                var urlList = urls.Take(_cacheOptions.Value.GetUrlLimit).Select(u => new GetUrlDto(u.GeneratedUrl, u.OriginalUrl, u.Creator, u.Category, u.TotalVisits)).ToList();
                var ttl = TimeSpan.FromMinutes(_cacheOptions.Value.GetAllTtlMinutes);
                var jitter = TimeSpan.FromSeconds(Random.Shared.Next(_cacheOptions.Value.GetAllJitterMinValue, _cacheOptions.Value.GetAllJitterMaxValue));
        
                _cache.Set(key, urlList, ttl + jitter);

                var topUrls = urls.OrderByDescending(u => u.TotalVisits).Take(_cacheOptions.Value.GetUrlLimit);
                ttl = TimeSpan.FromHours(_cacheOptions.Value.GetUrlTtlHours);
                jitter = TimeSpan.FromSeconds(Random.Shared.Next(_cacheOptions.Value.GetUrlJitterMinValue, _cacheOptions.Value.GetUrlJitterMaxValue));

                foreach (var url in topUrls)
                {
                    _cache.Set($"urls:url={url.GeneratedUrl}", url.OriginalUrl, ttl + jitter);
                }

                _logger.LogInformation("Cache Warmup for Url succeeded!");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache Warmup failed!");
        }
    }
}