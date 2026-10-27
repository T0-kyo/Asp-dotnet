using Microsoft.Extensions.Options;
using UrlShortner.Repositories;
using UrlShortner.Settings;


namespace UrlShortner.BackgroundServices;

public class RepositoryCleanupService : BackgroundService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IOptions<BackgroundSettings> _options;
    private readonly ILogger<RepositoryCleanupService> _logger;


    public RepositoryCleanupService(IUrlRepository urlRepository, IOptions<BackgroundSettings> options, ILogger<RepositoryCleanupService> logger)
    {
        _urlRepository = urlRepository;
        _options = options;
        _logger =logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _urlRepository.RemoveExpiredAsync();

                await Task.Delay(TimeSpan.FromDays(_options.Value.RepositoryCleanupIntervalDays), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error from repository cleanup service.");
            }
        }
    }
}