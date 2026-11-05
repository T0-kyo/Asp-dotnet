using UrlShortner.DTOs;
using UrlShortner.Models;
using UrlShortner.Stores;
using UrlShortner.Settings;
using UrlShortner.Exceptions;
using UrlShortner.Models.Types;
using UrlShortner.Repositories;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;


namespace UrlShortner.Services;

public class UrlService : IUrlService
{
    private readonly IOptions<UrlDefaultSettings> _urlOptions;
    private readonly IOptions<CacheSettings> _cacheOptions;
    private readonly IUrlRepository _urlRepository;
    private readonly IInFlightStore _inFlightStore;
    private readonly ILogger<UrlService> _logger;
    private readonly IVisitStore _visitStore;
    private readonly IMemoryCache _cache;
    static private int _cacheVersion = 1;
    const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";


    public UrlService(
        IOptions<UrlDefaultSettings> urlOptions, 
        IVisitStore visitStore, 
        ILogger<UrlService> logger, 
        IOptions<CacheSettings> cacheOptions, 
        IUrlRepository urlRepository, 
        IMemoryCache cache, 
        IInFlightStore inFlightStore)
    {
        _urlRepository = urlRepository;
        _inFlightStore =inFlightStore;
        _cacheOptions = cacheOptions;
        _urlOptions = urlOptions;
        _visitStore = visitStore;
        _logger = logger;
        _cache = cache;
    }

    private string GenerateShortUrl()
    {
        return RandomNumberGenerator.GetString(allowedChars, _urlOptions.Value.UrlRandomValues);
    }

    private bool ValidateUri(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var result))
        {
            return result.Scheme == Uri.UriSchemeHttp 
            || result.Scheme == Uri.UriSchemeHttps;
        }

        return false;
    }

    public async Task<Url> AddUrlAsync(AddUrlDto dto)
    {
        if (!ValidateUri(dto.OriginalUrl))
            throw new InvalidUri();

        var url = new Url
        {
            OriginalUrl = dto.OriginalUrl,
            GeneratedUrl = GenerateShortUrl(),
            Creator = dto.Creator ?? _urlOptions.Value.DefaultCreator,
            Category = dto.Category ?? UrlCategory.Others,
            ExpiresAt = DateTime.UtcNow.Add(TimeSpan.FromDays(_urlOptions.Value.UrlExpireInDays))
        };

        var attempts = _urlOptions.Value.UrlGenerationMaxAttempts;

        while (!await _urlRepository.TryAddAsync(url) && attempts > 0)
        {
            url = new Url
            {
                OriginalUrl = dto.OriginalUrl,
                GeneratedUrl = GenerateShortUrl(),
                Creator = dto.Creator ?? _urlOptions.Value.DefaultCreator,
                Category = dto.Category ?? UrlCategory.Others,
                ExpiresAt = DateTime.UtcNow.Add(TimeSpan.FromDays(_urlOptions.Value.UrlExpireInDays))
            };

            attempts--;
        }

        if (attempts == 0)
        {
            _logger.LogCritical("Failed to generate Url. Out of attempts!");

            throw new FailedToGenerateUrl();
        }

        Interlocked.Increment(ref _cacheVersion);
        return url;
    }

    public async Task RemoveUrlAsync(string url)
    {
        string key = $"urls:url={url}";

        if (!await _urlRepository.RemoveAsync(url))
            throw new UrlNotFound();
        
        _cache.Remove(key);
    }

    public async Task<string> GetOriginalUrlAsync(string shortUrl)
    {
        var key = $"urls:url={shortUrl}";

        if (_cache.TryGetValue(key, out string? cached) && cached is not null)
        {
            _visitStore.AddVisitCount(shortUrl);

            return cached;
        }

        var lazy = _inFlightStore.GetOrAddUrl(key, async () =>
        {
            try
            {
                var url = await _urlRepository.GetAsync(shortUrl) ?? throw new UrlNotFound();

                var jitter = TimeSpan.FromSeconds(Random.Shared.Next(_cacheOptions.Value.GetUrlJitterMinValue, _cacheOptions.Value.GetUrlJitterMaxValue));
                _cache.Set(key, url.OriginalUrl, TimeSpan.FromHours(_cacheOptions.Value.GetUrlTtlHours) + jitter);
            
                return url.OriginalUrl;
            }
            finally
            {
                _inFlightStore.TryRemove(key);
            }
        });

        var result = await lazy.Value;
        
        _visitStore.AddVisitCount(shortUrl);
        return result;
    }

    public async Task<IReadOnlyCollection<GetUrlDto>> GetAllUrlsAsync(string? sortBy, string? creator, string? category, int page, int pageSize)
    {
        string key = $"urls:v{_cacheVersion}:creator={creator}:category={category}:page={page}:pageSize={pageSize}";

        if (_cache.TryGetValue(key, out IReadOnlyCollection<GetUrlDto>? cached) && cached is not null)
            return cached;

        var lazy = _inFlightStore.GetOrAddUrlList(key, async () =>
        {
            try
            {
                var urls = await _urlRepository.GetAllAsync();

                if (creator is not null)
                {
                    urls = [..urls.Where(u => u.Creator == creator)];
                }

                if (category is not null)
                {
                    var urlCategory = category.ToLower() switch
                    {
                        "business" => UrlCategory.Business,
                        "entertainment" => UrlCategory.Entertainment,
                        "online-tools" => UrlCategory.OnlineTools,
                        "social-media" => UrlCategory.SocialMedia,
                        "others" => UrlCategory.Others,
                        _ => throw new FilterNotFound($"category={category}") 
                    };

                    urls = [..urls.Where(u => u.Category == urlCategory)];
                }

                if (sortBy is not null)
                {
                    switch (sortBy.ToLower())
                    {
                        case "latest":
                            urls = [..urls.OrderByDescending(u => u.CreatedAt)];
                            break;
                        case "oldest":
                            urls = [..urls.OrderBy(u => u.CreatedAt)];
                            break;
                        case "most-popular":
                            urls = [..urls.OrderByDescending(u => u.TotalVisits)];
                            break;
                        case "least-popular":
                            urls = [..urls.OrderBy(u => u.TotalVisits)];
                            break;
                        default:
                            throw new FilterNotFound($"sortBy={sortBy}");
                    }
                }

                urls = [..urls.Skip((page - 1) * pageSize).Take(pageSize)];
                var dtos = urls.Select(u => new GetUrlDto(u.GeneratedUrl, u.OriginalUrl, u.Creator, u.Category, u.TotalVisits)).ToList();

                var jitter = TimeSpan.FromSeconds(Random.Shared.Next(_cacheOptions.Value.GetAllJitterMinValue, _cacheOptions.Value.GetAllJitterMaxValue));
                _cache.Set(key, dtos, TimeSpan.FromMinutes(_cacheOptions.Value.GetAllTtlMinutes) + jitter);

                return dtos;
            }
            finally
            {
                _inFlightStore.TryRemove(key);
            }
        });

        var result = await lazy.Value;

        return result;
    }
}