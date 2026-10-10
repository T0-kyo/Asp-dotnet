using UrlShortner.Models.Types;


namespace UrlShortner.Models;

public class Url
{
    private int _totalVisits;


    public int TotalVisits => _totalVisits;
    public string OriginalUrl { get; set; } = "";
    public string GeneratedUrl { get; set; } = "";
    public string Creator { get; set; } = "";
    public UrlCategory Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }


    public void IncreaseVisitCount()
    {
        Interlocked.Increment(ref _totalVisits);
    }
}