namespace UrlShortner.Settings;

public class CacheSettings
{
    public int GetUrlLimit { get; set; }
    public int GetAllTtlMinutes { get; set; }
    public int GetAllJitterMinValue { get; set; }
    public int GetAllJitterMaxValue { get; set; }
    public int GetUrlTtlHours { get; set; }
    public int GetUrlJitterMinValue { get; set; }
    public int GetUrlJitterMaxValue { get; set; }
}