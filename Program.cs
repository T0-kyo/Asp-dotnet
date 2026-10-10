using UrlShortner.Services;
using UrlShortner.Settings;
using UrlShortner.Middleware;
using UrlShortner.Repositories;
using UrlShortner.BackgroundServices;
using Microsoft.AspNetCore.RateLimiting;



var builder = WebApplication.CreateBuilder(args);



builder.Services.Configure<AdminSettings>(
    builder.Configuration.GetSection("Admin-Key"));

builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));

builder.Services.Configure<FilterSettings>(
    builder.Configuration.GetSection("FilterSettings"));

builder.Services.Configure<UrlDefaultSettings>(
    builder.Configuration.GetSection("UrlDefaultSettings"));

builder.Services.Configure<RateLimiterSettings>(
    builder.Configuration.GetSection("RateLimiterSettings"));


var rateLimiterOptions = new RateLimiterSettings();

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    limiterOptions.AddFixedWindowLimiter(policyName: "fixed_window", options =>
    {
        options.PermitLimit = rateLimiterOptions.MaxRequests;
        options.Window = TimeSpan.FromMinutes(rateLimiterOptions.WindowTtlMinutes);
    });
});


builder.Services.AddSingleton<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();

builder.Services.AddHostedService<CacheWarmupService>();
builder.Services.AddHostedService<RepositoryCleanupService>();
builder.Services.AddHostedService<IdempotencyCleanupService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWhen(
    context => context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase),
    appBuilder => appBuilder.UseMiddleware<IdempotencyMiddleware>()
);
app.UseExceptionHandler();
app.UseMiddleware<AdminAuthorizationMiddleware>();

app.UseRouting();
app.UseRateLimiter();

app.MapControllers();



app.Run();
