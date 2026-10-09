using UrlShortner.Services;
using UrlShortner.Middleware;
using UrlShortner.Controllers;
using UrlShortner.Repositories;




var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();


var app = builder.Build();


app.UseExceptionHandler();
app.MapControllers();


app.Run();
