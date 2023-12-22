using System;
using AutoProxy.PortManager.Interfaces;
using AutoProxy.PortManager.Logging;
using AutoProxy.PortManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = new ConfigurationBuilder();
var serviceProvider = new ServiceCollection()
    .AddLogging(logging => logging.AddSpecterConsoleLogger(configuration =>
    {
        configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.DarkCyan;
        configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
    }))
    .AddSingleton<ITimerService, TimerService>()
    .AddSingleton<SyncService>()
    .BuildServiceProvider();

var timerService = serviceProvider.GetService<ITimerService>();
timerService?.Start();

Console.ReadKey();