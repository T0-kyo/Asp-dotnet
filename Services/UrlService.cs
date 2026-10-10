using System.Text;
using UrlShortner.DTOs;
using UrlShortner.Models;
using UrlShortner.Settings;
using UrlShortner.Exceptions;
using UrlShortner.Models.Types;
using UrlShortner.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;


namespace UrlShortner.Services;

public class UrlService : IUrlService
{
    private readonly IOptions<UrlDefaultSettings> _urlOptions;
    private readonly IOptions<CacheSettings> _cacheSettings;
    private readonly IUrlRepository _urlRepository;
    private readonly IMemoryCache _cache;
    private int _cacheVersion = 1;


    public UrlService(IOptions<UrlDefaultSettings> urlOptions, IOptions<CacheSettings> cacheSettings, IUrlRepository urlRepository, IMemoryCache cache)
    {
        _urlRepository = urlRepository;
        _cacheSettings = cacheSettings;
        _urlOptions = urlOptions;
        _cache = cache;
    }

    private string GenerateShortUrl(string url)
    {
        var builder = new StringBuilder();

        builder
        .Append(url[0])
        .Append(url.Length)
        .Append(url[^1])
        .Append(Random.Shared.GetHexString(_urlOptions.Value.UrlRandomValues));

        return builder.ToString();
    }

    public async Task<Url> AddUrlAsync(AddUrlDto dto)
    {
        var url = new Url
        {
            OriginalUrl = dto.OriginalUrl,
            GeneratedUrl = GenerateShortUrl(dto.OriginalUrl),
            Creator = dto.Creator ?? _urlOptions.Value.DefaultCreator,
            Category = dto.Category ?? UrlCategory.Others
        };

        while (!await _urlRepository.TryAddAsync(url))
        {
            url.GeneratedUrl = GenerateShortUrl(dto.OriginalUrl);
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

        var result = await _cache.GetOrCreateAsync(key, async enrty =>
        {
            enrty.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_cacheSettings.Value.GetUrlTtlHours);

            var url = await _urlRepository.GetAsync(shortUrl);

            if (url is null)
                throw new UrlNotFound();

            url.IncreaseVisitCount();
            return url.OriginalUrl;
        });

        return result!;
    }

    public async Task<IReadOnlyCollection<GetUrlDto>> GetAllUrlsAsync(string? sortBy, string? creator, string? category, int page, int pageSize)
    {
        string key = $"urls:v{_cacheVersion}:creator={creator}:category={category}:page={page}:pageSize={pageSize}";

        var result = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.Value.GetAllTtlMinutes);

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

            urls = [..urls.Skip(page * pageSize).Take(pageSize)];

            return urls.Select(u => new GetUrlDto(u.OriginalUrl, u.Creator, u.Category)).ToList();
        });
        
        return result!;
    }
}