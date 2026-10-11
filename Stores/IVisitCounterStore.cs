namespace UrlShortner.Stores;

public interface IVisitStore
{
    IReadOnlyCollection<KeyValuePair<string, int>> Drain();
    void AddVisitCount(string url);
}