namespace UrlShortner.Settings;

public class IdempotencySettings
{
    public int TtlHours { get; set; }
    public int StaleProcessingMinutes { get; set; }
    public int CleanupIntervalMinutes { get; set; }
}