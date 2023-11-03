using System;
using System.Threading;
using PortUpdate.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PortUpdate.Services;

public class TimerService : ITimerService
{
    private Timer timer;
    private readonly ILogger<TimerService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly int intervalSeconds;

    public TimerService(ILogger<TimerService> logger,IServiceScopeFactory factory, IConfiguration configuration)
    {
        _logger = logger;
        _factory = factory;
        intervalSeconds = 30;
        timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    private async void TimerCallback(Object o)
    {
        _logger.LogDebug("Timer callback executed at " + DateTime.Now);
        await using var asyncScope = _factory.CreateAsyncScope();
        var portService = asyncScope.ServiceProvider.GetRequiredService<SyncService>();
        portService.Update();

    }

    public void Start()
    {
        _logger.LogInformation("Timer service started.");
    }
}