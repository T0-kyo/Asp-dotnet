using System.Net;


namespace UrlShortner.Exceptions;

public class MissingAdminKey : BaseException
{
    public MissingAdminKey() : base("Error: Wrong or missing admin key", HttpStatusCode.Unauthorized)
    {
    }
}