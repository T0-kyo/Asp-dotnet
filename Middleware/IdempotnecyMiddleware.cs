using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using UrlShortner.Models.Types;
using UrlShortner.Settings;
using UrlShortner.Models;
using UrlShortner.Stores;
using System.Text;
using System.Net;


namespace UrlShortner.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _store;
    private readonly IOptions<IdempotencySettings> _options;
    private readonly ILogger<IdempotencyMiddleware> _logger;


    public IdempotencyMiddleware(RequestDelegate next, IIdempotencyStore store, IOptions<IdempotencySettings> options, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _store = store;
        _logger = logger;
        _options = options;
    }

    private string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        StringValues idempotencyKey = "";

        try
        {
            if (httpContext.Request.Headers.TryGetValue("Idempotency-Key", out idempotencyKey))
            {
                httpContext.Request.EnableBuffering();
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;

                var record = new IdempotencyRecord
                {
                    Key = idempotencyKey!,
                    BodyHash = ComputeHash(body),
                    Status = IdempotencyStatus.Pending,
                };

                if (await _store.TryAddAsync(idempotencyKey!, record))
                {
                    var existingRecord = await _store.GetRecordAsync(idempotencyKey!);

                    if (existingRecord is not null)
                    {
                        if (existingRecord.BodyHash != record.BodyHash)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                            httpContext.Response.ContentType = "text/plain";
                            await httpContext.Response.WriteAsync("Used Idempotency Key on different body!");
                            return;
                        }

                        if (existingRecord.Status == IdempotencyStatus.Completed)
                        {
                            var statusCode = existingRecord.StatusCode ?? HttpStatusCode.InternalServerError;

                            httpContext.Response.StatusCode = (int)statusCode;

                            httpContext.Response.Headers.Clear();

                            foreach (var header in existingRecord.ResponseHeaders ?? new())
                            {
                                if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                httpContext.Response.Headers.Append(header.Key, header.Value);
                            }

                            await httpContext.Response.WriteAsync(existingRecord.ResponseBody ?? "");
                            return;
                        }
                        else if (DateTime.UtcNow - existingRecord.CreatedAt < TimeSpan.FromMinutes(_options.Value.StaleProcessingMinutes))
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                            httpContext.Response.ContentType = "text/plain";
                            await httpContext.Response.WriteAsync("Request is being processed!");
                            return;
                        }
                        else
                        {
                            await _store.UpdateAsync(idempotencyKey!, record);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error: in Idempotency middleware");
        }

        var originalBody = httpContext.Response.Body;
        using var memoryStream = new MemoryStream();
        httpContext.Response.Body = memoryStream;

        try
        {
            await _next(httpContext);

            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var response = await _store.GetRecordAsync(idempotencyKey!);

                if (response is not null)
                {
                    response.Status = IdempotencyStatus.Completed;
                    response.StatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
                    response.ResponseBody = Encoding.UTF8.GetString( memoryStream.ToArray());
                    response.ResponseHeaders = httpContext.Response.Headers.ToDictionary();

                    await _store.UpdateAsync(idempotencyKey!, response);
                }
            }
        }
        finally
        {   
            httpContext.Response.Body = originalBody;
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBody);
        }
    }
}