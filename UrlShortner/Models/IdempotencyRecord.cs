using Microsoft.Extensions.Primitives;
using UrlShortner.Models.Types;
using System.Net;


namespace UrlShortner.Models;

public class IdempotencyRecord
{
    public string Key { get; set; } = "";
    public string BodyHash { get; set; } = "";
    public IdempotencyStatus Status { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public Dictionary<string, StringValues>? ResponseHeaders { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}