using UrlShortner.Models.Types;


namespace UrlShortner.Models;

public class Url
{
    private int _totalVisits;


    public int TotalVisits => _totalVisits;
    public string OriginalUrl { get; init; } = "";
    public string GeneratedUrl { get; init; } = "";
    public string Creator { get; init; } = "";
    public UrlCategory Category { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }


    public void IncreaseVisitCount(int value)
    {
        Interlocked.Add(ref _totalVisits, value);
    }
}