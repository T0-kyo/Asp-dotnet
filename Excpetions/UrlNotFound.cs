using System.Net;


namespace UrlShortner.Exceptions;

public class UrlNotFound : BaseException
{
    public UrlNotFound() : base("Error: Uri not found!", HttpStatusCode.NotFound)
    {
    }
}