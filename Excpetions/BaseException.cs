using System.Net;


namespace UrlShortner.Exceptions;

public class BaseException : Exception
{
    public HttpStatusCode StatusCode { get; init; }

    public BaseException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        StatusCode = statusCode;
    }
}