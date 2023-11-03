using System;
using PortUpdate;
using PortUpdate.Interfaces;
using PortUpdate.Logging;
using PortUpdate.Services;
using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

var builder = new ConfigurationBuilder();

var configuration = builder.Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(logging => logging.AddSpecterConsoleLogger(configuration =>
    {
        // Replace warning value from appsettings.json of "Cyan"
        configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.DarkCyan;
        // Replace warning value from appsettings.json of "Red"
        configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
    }))
    .AddSingleton<ITimerService, TimerService>()
    .AddSingleton<ProxmoxService>()
    .AddSingleton<DockerService>()
    .AddTransient<SyncService>()
    /*.AddSingleton(new PveClient("192.168.178.66").ApiToken = "root@pam!858cc9ac-b8d2-42f7-a474-b2a193b18765")*/
    .BuildServiceProvider();

var dataAccess = serviceProvider.GetService<SyncService>();
dataAccess?.Init();

var timerService = serviceProvider.GetService<ITimerService>();
timerService?.Start();

Console.ReadKey();