using Microsoft.Extensions.Options;
using UrlShortner.Exceptions;
using UrlShortner.Settings;


namespace UrlShortner.Middleware;

public class AdminAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<AdminSettings> _options;


    public AdminAuthorizationMiddleware(RequestDelegate next, IOptions<AdminSettings> options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Headers["Admin-Key"] != _options.Value.Key)
            throw new MissingAdminKey();

        await _next(httpContext);
    }
}