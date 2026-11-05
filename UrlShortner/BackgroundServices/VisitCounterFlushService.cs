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
    private readonly ILogger<VisitCounterFlushService> _logger;


    public VisitCounterFlushService(IVisitStore visitStore, IUrlRepository urlRepository, IOptions<BackgroundSettings> options, ILogger<VisitCounterFlushService> logger)
    {
        _logger = logger;
        _options = options;
        _visitStore = visitStore;
        _urlRepository = urlRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.Value.VisitCountFlushIntervalSeconds), stoppingToken);

                await _urlRepository.IncrementVisitCountAsync(_visitStore.Drain());   
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error from visit counter flush service.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _urlRepository.IncrementVisitCountAsync(_visitStore.Drain());   
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in shutdown from visit flush service");
        }
        finally
        {
            await base.StopAsync(cancellationToken);   
        }
    }
}