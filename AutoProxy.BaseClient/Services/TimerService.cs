﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortUpdate.Interfaces;

namespace AutoProxy.BaseClient.Services;

public class TimerService : ITimerService
{
    private Timer timer;
    private readonly ILogger<TimerService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly int intervalSeconds;

    public TimerService(ILogger<TimerService> logger,IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;
        intervalSeconds = 5;
        timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    private async void TimerCallback(Object o)
    {
        _logger.LogDebug("Timer callback executed at " + DateTime.Now);
        var scope = _factory.CreateScope();
        var portClient = scope.ServiceProvider.GetRequiredService<PortClient>();
        portClient.GetPorts();
    }

    public void Start()
    {
        _logger.LogInformation("Timer service started.");
    }
}