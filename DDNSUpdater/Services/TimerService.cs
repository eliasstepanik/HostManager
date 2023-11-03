using System;
using System.Threading;
using DDNSUpdater.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DDNSUpdater.Services;

public class TimerService : ITimerService
{
    private PeriodicTimer _timer;
    private readonly ILogger<TimerService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly int intervalSeconds;

    public TimerService(ILogger<TimerService> logger,IServiceScopeFactory factory, IConfiguration configuration)
    {
        _logger = logger;
        _factory = factory;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(configuration.GetValue<int>("TimerIntervalSeconds")));
    }


    private async Task Timer()
    {
        _logger.LogInformation("Timer callback executed at " + DateTime.Now);
        using var asyncScope = _factory.CreateAsyncScope();
        var ddnsService = asyncScope.ServiceProvider.GetRequiredService<DDNSService>();
        var dockerService = asyncScope.ServiceProvider.GetRequiredService<DockerService>();

        bool changed = await dockerService.UpdateDomainList();
        await ddnsService.Update(changed);
    }
    public async Task Start()
    {
        _logger.LogInformation("Timer service started.");

        while (await _timer.WaitForNextTickAsync())
        {
            await Timer();
        }
    }
}