﻿using AutoProxy.BaseClient.Logging;
using AutoProxy.BaseClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortUpdate.Interfaces;


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
    .AddSingleton<PortClient>()
    .BuildServiceProvider();

var timerService = serviceProvider.GetService<ITimerService>();
timerService?.Start();

Console.ReadKey();