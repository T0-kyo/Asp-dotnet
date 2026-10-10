using UrlShortner.Models.Types;


namespace UrlShortner.DTOs;

public record GetUrlDto(string OriginalUrl, string Creator, UrlCategory Category, int VisitCount);