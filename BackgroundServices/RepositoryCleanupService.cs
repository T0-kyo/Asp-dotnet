using UrlShortner.Repositories;


namespace UrlShortner.BackgroundServices;

public class RepositoryCleanupService : BackgroundService
{
    private readonly IUrlRepository _urlRepository;


    public RepositoryCleanupService(IUrlRepository urlRepository)
    {
        _urlRepository = urlRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _urlRepository.RemoveExpired();

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}