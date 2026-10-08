using System.ComponentModel.DataAnnotations;
using UrlShortner.Models.Types;


namespace UrlShortner.DTOs;

public record AddUrlDto([Required] string OriginalUrl, string? Creator, UrlCategory? Category);