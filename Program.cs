using UrlShortner.Services;
using UrlShortner.Middleware;
using UrlShortner.Controllers;
using UrlShortner.Repositories;
using UrlShortner.BackgroundServices;




var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();

builder.Services.AddHostedService<CacheWarmupService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.MapControllers();


app.Run();
