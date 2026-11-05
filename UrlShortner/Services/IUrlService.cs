using UrlShortner.DTOs;
using UrlShortner.Models;


namespace UrlShortner.Services;

public interface IUrlService
{
    Task<Url> AddUrlAsync(AddUrlDto dto);
    Task RemoveUrlAsync(string url);
    Task<string> GetOriginalUrlAsync(string shortUrl);
    Task<IReadOnlyCollection<GetUrlDto>> GetAllUrlsAsync(string? sortBy, string? creator, string? category, int page, int pageSize);
}