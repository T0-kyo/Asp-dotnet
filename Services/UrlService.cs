using System.Text;
using UrlShortner.DTOs;
using UrlShortner.Models;
using UrlShortner.Exceptions;
using UrlShortner.Models.Types;
using UrlShortner.Repositories;
using Microsoft.Extensions.Caching.Memory;


namespace UrlShortner.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IMemoryCache _cache;
    private int _cacheVersion = 1;


    public UrlService(IUrlRepository urlRepository, IMemoryCache cache)
    {
        _urlRepository = urlRepository;
        _cache = cache;
    }

    private string GenerateShortUrl(string url)
    {
        var builder = new StringBuilder();

        builder
        .Append(url[0])
        .Append(url.Length)
        .Append(url[^1])
        .Append(Random.Shared.GetHexString(7));

        return builder.ToString();
    }

    public async Task<Url> AddUrlAsync(AddUrlDto dto)
    {
        var url = new Url
        {
            OriginalUrl = dto.OriginalUrl,
            GeneratedUrl = GenerateShortUrl(dto.OriginalUrl),
            Creator = dto.Creator ?? "Unknown",
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
            enrty.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);

            var url = await _urlRepository.GetAsync(shortUrl);

            if (url is null)
                throw new UrlNotFound();

            url.IncreaseVisitCount();
            return url.OriginalUrl;
        });

        return result!;
    }

    public async Task<IReadOnlyCollection<GetUrlDto>> GetAllUrlsAsync()
    {
        string key = $"urls:v{_cacheVersion}";

        var result = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            var urls = await _urlRepository.GetAllAsync();

            return urls.Select(u => new GetUrlDto(u.OriginalUrl, u.Creator, u.Category)).ToList();
        });
        
        return result!;
    }
}