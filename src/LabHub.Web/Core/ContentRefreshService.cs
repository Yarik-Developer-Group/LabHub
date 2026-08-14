using Microsoft.Extensions.Options;

namespace LabHub.Web.Core;

public sealed class ContentRefreshService : BackgroundService
{
    private readonly ContentRefresher _refresher;
    private readonly IOptionsMonitor<LabHubOptions> _options;
    private readonly ILogger<ContentRefreshService> _logger;

    public ContentRefreshService(ContentRefresher refresher, IOptionsMonitor<LabHubOptions> options,
        ILogger<ContentRefreshService> logger)
    {
        _refresher = refresher;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.CurrentValue;
        if (!options.GitHub.IsConfigured) return;
        await RefreshOnceAsync(stoppingToken);
        if (options.RefreshMinutes <= 0) return;
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.RefreshMinutes));
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RefreshOnceAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {

        }
    }

    private async Task RefreshOnceAsync(CancellationToken ct)
    {
        if (await _refresher.RefreshAsync(ct))
            _logger.LogInformation("Content refreshed from GitHub");
        else
            _logger.LogWarning("Content refresh failed: {Error}", _refresher.LastError);
    }
}
