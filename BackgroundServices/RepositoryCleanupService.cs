using Microsoft.Extensions.Options;
using UrlShortner.Repositories;
using UrlShortner.Settings;


namespace UrlShortner.BackgroundServices;

public class RepositoryCleanupService : BackgroundService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IOptions<BackgroundSettings> _options;


    public RepositoryCleanupService(IUrlRepository urlRepository, IOptions<BackgroundSettings> options)
    {
        _urlRepository = urlRepository;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _urlRepository.RemoveExpiredAsync();

            await Task.Delay(TimeSpan.FromDays(_options.Value.RepositoryCleanupIntervalDays), stoppingToken);
        }
    }
}