using UrlShortner.Stores;
using UrlShortner.Settings;
using UrlShortner.Repositories;
using Microsoft.Extensions.Options;


namespace UrlShortner.BackgroundServices;

public class VisitCounterFlushService : BackgroundService
{
    private readonly IVisitStore _visitStore;
    private readonly IUrlRepository _urlRepository;
    private readonly IOptions<BackgroundSettings> _options;


    public VisitCounterFlushService(IVisitStore visitStore, IUrlRepository urlRepository, IOptions<BackgroundSettings> options)
    {
        _options = options;
        _visitStore = visitStore;
        _urlRepository = urlRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.Value.VisitCountFlushIntervalSeconds), stoppingToken);

            await _urlRepository.IncrementVisitCountAsync(_visitStore.Drain());
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _urlRepository.IncrementVisitCountAsync(_visitStore.Drain());
        await base.StopAsync(cancellationToken);
    }
}