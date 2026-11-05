using System.Net;


namespace UrlShortner.Exceptions;

public class FilterNotFound : BaseException
{
    public FilterNotFound(string filter) : base($"Error: filter ({filter}) doesn't exist!", HttpStatusCode.NotFound)
    {
    }
}