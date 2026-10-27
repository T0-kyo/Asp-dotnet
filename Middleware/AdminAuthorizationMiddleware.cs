using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using UrlShortner.Exceptions;
using UrlShortner.Settings;
using System.Text;


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
        if (!httpContext.Request.Headers.TryGetValue("Admin-Key", out var key) || string.IsNullOrEmpty(_options.Value.Key))
            throw new MissingAdminKey();

        byte[] keyBytes = Encoding.UTF8.GetBytes(key!);
        byte[] optionsKeyBytes = Encoding.UTF8.GetBytes(_options.Value.Key);

        if (!CryptographicOperations.FixedTimeEquals(keyBytes, optionsKeyBytes))
            throw new MissingAdminKey();

        await _next(httpContext);
    }
}