namespace UrlShortner.Settings;

public class UrlDefaultSettings
{
    public int UrlRandomValues { get; set; }
    public int UrlExpireInDays { get; set; }
    public string DefaultCreator { get; set; } = "";
    public int UrlGenerationMaxAttempts { get; set; }
}