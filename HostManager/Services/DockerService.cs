using Docker.DotNet;
using Docker.DotNet.Models;
using Grpc.Core;

namespace HostManager.Services;

public class DockerService : HostManager.DockerService.DockerServiceBase
{
    private readonly ILogger<DockerService> _logger;
    private readonly DockerClient _client;
    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _client = new DockerClientConfiguration()
            .CreateClient();

    }
    
    public async Task<GetPortsRequest?> GetPorts()
    {
        var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters());

        var portsRequest = new GetPortsRequest()
        {
            Ports = new List<Port>()
        };
        foreach (var container in containers)
        {
            foreach (var containerPort in container.Ports)
            {
                if (containerPort.PublicPort != 0 && containerPort.PublicPort != 443 && containerPort.PublicPort != 80)
                {
                    Port port = new Port()
                    {
                        Proto = containerPort.Type,
                        Dport = containerPort.PublicPort
                    };
                    if(!portsRequest.Ports.Contains(port))
                        portsRequest.Ports.Add(port);
                }
                
            }
        }
        
        return portsRequest;
    }
}