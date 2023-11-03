using System.Net.Http.Headers;
using PortUpdate.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortUpdate.Models;
using RestSharp;
using RestSharp.Serializers;
using Spectre.Console;
using Console = Spectre.Console.AnsiConsole;
using Port = PortUpdate.Models.Port;

namespace PortUpdate.Services;

public class SyncService : IPortService
{
    
    private readonly ILogger<SyncService> _logger;
    private readonly DockerService _docker;
    private readonly ProxmoxService _proxmox;


    public SyncService(ILogger<SyncService> logger, DockerService docker, ProxmoxService proxmox)
    {
        _logger = logger;
        _docker = docker;
        _proxmox = proxmox;
    }


    public async void Init()
    {

    }

    public async void Update()
    {
        var newPorts = await _docker.GetPorts();
        var proxmoxTicket = await _proxmox.GetTicket();
        var currentPorts = await _proxmox.GetPorts(proxmoxTicket ?? throw new Exception("Got no Ticket"));
        
    

        if (newPorts == null || currentPorts == null)
        {
            _logger.LogError("Update Failed");
            return;
        };
        
        ProcessPortRequest(currentPorts, newPorts, proxmoxTicket);
    }

    private static bool ExistsIn(Port port, PortRequest destination)
    {
        foreach (var dPort in destination.Ports)
        {
            if (dPort.Dport == port.Dport && dPort.Proto == port.Proto) return true;
        }

        return false;
    }
    
    private void ProcessPortRequest(PortRequest currentPorts,PortRequest newPorts, TicketRequest proxmoxTicket)
    {
        
        // Fetch the current list of ports from Proxmox

        // Remove ports that are present in currentPorts but not in portRequest.Ports (based on Pos and Proto)
        foreach (var currentPort in currentPorts.Ports)
        {
            bool shouldRemove = !newPorts.Ports.Any(requestedPort =>
                requestedPort.Dport == currentPort.Dport && requestedPort.Proto == currentPort.Proto);

            if (shouldRemove)
            {
                _proxmox.RemovePort(currentPort.Pos, proxmoxTicket);
            }
        }

        // Add ports that are present in portRequest.Ports but not in currentPorts (based on Pos and Proto)
        foreach (var port in newPorts.Ports)
        {
            bool shouldAdd = !currentPorts.Ports.Any(currentPort =>
                currentPort.Dport == port.Dport && currentPort.Proto == port.Proto);

            if (shouldAdd)
            {
                _proxmox.AddPort(new Port()
                {
                    Dport = port.Dport,
                    Proto = port.Proto
                }, proxmoxTicket);
            }
        }
    }

    

}
