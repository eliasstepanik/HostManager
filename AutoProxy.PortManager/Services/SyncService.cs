using AutoProxy.Server.Docker;
using AutoProxy.Server.Proxmox;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace AutoProxy.PortManager.Services;

public class SyncService(ILogger<SyncService> logger)
{
    private GrpcChannel _channel = null!;

    public void Update()
    {
        var dockerClient = new DockerService.DockerServiceClient(_channel);
        var proxmoxClient = new ProxmoxService.ProxmoxServiceClient(_channel);
        
        var dockerPorts = dockerClient.GetPorts(new Server.Docker.GetPortsRequest());
        var proxmoxPorts = proxmoxClient.GetPorts(new Server.Proxmox.GetPortsRequest());
        
        

        if (dockerPorts == null || proxmoxPorts == null)
        {
            logger.LogError("Update Failed");
            return;
        }
        
        ProcessPortRequest(proxmoxPorts, dockerPorts);
    }
    
    private void ProcessPortRequest(Server.Proxmox.GetPortsReply currentPorts,Server.Docker.GetPortsReply newPorts)
    {
        _channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("GRPC_HOST") ?? throw new Exception("GRPC Host not set!"));
        var proxmoxClient = new ProxmoxService.ProxmoxServiceClient(_channel);
        
        foreach (var currentPort in currentPorts.Ports)
        {
            bool shouldRemove = !newPorts.Ports.Any(requestedPort =>
                requestedPort.DestPort == currentPort.DestPort && (int)requestedPort.Protocol == (int)currentPort.Protocol);

            if (shouldRemove)
            {
                proxmoxClient.ClosePortAsync(new ClosePortRequest()
                {
                    Position = currentPort.Postion,
                    Group = Environment.GetEnvironmentVariable("PROXMOX_FIREWALL_GROUP")
                });
            }
        }

        // Add ports that are present in portRequest.Ports but not in currentPorts (based on Pos and Proto)
        foreach (var port in newPorts.Ports)
        {
            bool shouldAdd = !currentPorts.Ports.Any(currentPort =>
                currentPort.DestPort == port.DestPort && (int)currentPort.Protocol == (int)port.Protocol);

            if (shouldAdd)
            {
                proxmoxClient.OpenPortAsync(new OpenPortRequest()
                {
                    Group = Environment.GetEnvironmentVariable("PROXMOX_FIREWALL_GROUP"),
                    Port = new OpenPortRequest.Types.Port()
                    {
                        DestPort = port.DestPort,
                        Protocol = (OpenPortRequest.Types.Port.Types.ProtocolType) port.Protocol
                    }
                });
            }
        }
    }
}
