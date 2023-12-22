using System;
using System.Threading;
using AutoProxy.PortManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoProxy.PortManager.Services;
public class TimerService : ITimerService
{
    private Timer _timer;
    private readonly ILogger<TimerService> _logger;
    private readonly IServiceScopeFactory _factory;

    public TimerService(ILogger<TimerService> logger,IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;
        var intervalSeconds = 5;
        _timer = new Timer(TimerCallback!, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    private async void TimerCallback(Object o)
    {
        _logger.LogDebug("Timer callback executed at {Date}", DateTime.Now);
        var scope = _factory.CreateScope();
        var portClient = scope.ServiceProvider.GetRequiredService<SyncService>();
        portClient.Update();
    }

    public void Start()
    {
        _logger.LogInformation("Timer service started");
    }
}