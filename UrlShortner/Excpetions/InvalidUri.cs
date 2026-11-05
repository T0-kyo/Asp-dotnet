using System.Net;


namespace UrlShortner.Exceptions;

public class InvalidUri : BaseException
{
    public InvalidUri() : base("Invalid Uri!", HttpStatusCode.BadRequest)
    {
    }
}