using System.Net;


namespace UrlShortner.Exceptions;

public class FailedToGenerateUrl : BaseException
{
    public FailedToGenerateUrl() : base($"Failed to Generate Url. Please try again later.", HttpStatusCode.InternalServerError)
    {
    }
}