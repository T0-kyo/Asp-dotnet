namespace UrlShortner.Settings; 

public class RateLimiterSettings
{
    public int MaxRequests { get; set; }
    public int WindowTtlMinutes { get; set; }
}