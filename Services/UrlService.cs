using UrlShortner.DTOs;
using UrlShortner.Models;
using UrlShortner.Models.Types;
using UrlShortner.Repositories;
using UrlShortner.Exceptions;
using System.Text;


namespace UrlShortner.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;


    public UrlService(IUrlRepository urlRepository)
    {
        _urlRepository = urlRepository;
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

        return url;
    }

    public async Task RemoveUrlAsync(string url)
    {
        if (!await _urlRepository.RemoveAsync(url))
            throw new UrlNotFound();
    }

    public async Task<string> GetOriginalUrlAsync(string shortUrl)
    {
        var url = await _urlRepository.GetAsync(shortUrl);

        if (url is null)
            throw new UrlNotFound();
        
        url.IncreaseVisitCount();
        return url.OriginalUrl;
    }

    public async Task<IReadOnlyCollection<GetUrlDto>> GetAllUrlsAsync()
    {
        var urls = await _urlRepository.GetAllAsync();

        return [..urls.Select(u => new GetUrlDto(u.OriginalUrl, u.Creator, u.Category))];
    }
}