using UrlShortner.Middleware;
using UrlShortner.Repositories;
using UrlShortner.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();


var app = builder.Build();


app.UseMiddleware<GlobalExceptionHandler>();
app.MapControllers();


app.Run();
