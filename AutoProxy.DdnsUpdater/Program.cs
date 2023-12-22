using AutoProxy.DdnsUpdater;
using AutoProxy.DdnsUpdater.Interfaces;
using AutoProxy.DdnsUpdater.Logging;
using AutoProxy.DdnsUpdater.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = new ConfigurationBuilder();
var serviceProvider = new ServiceCollection()
    .AddLogging(logging => logging.AddSpecterConsoleLogger(configuration =>
    {
        // Replace warning value from appsettings.json of "Cyan"
        configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.DarkCyan;
        // Replace warning value from appsettings.json of "Red"
        configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
    }))
    .AddSingleton<ITimerService, TimerService>()
    .AddSingleton<DdnsUpdater>()
    .AddDbContext<DdnsDbContext>(s => s.UseInMemoryDatabase("ddnsUpdater")) 
    .BuildServiceProvider();

var timerService = serviceProvider.GetService<ITimerService>();
timerService?.Start();

var ddnsService = serviceProvider.GetService<DdnsUpdater>();
ddnsService?.Start();


Console.ReadKey();