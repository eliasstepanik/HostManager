using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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
    
    public override async Task<GetPortsReply> GetPorts(GetPortsRequest request, ServerCallContext context)
    {
        var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters());

        var ports = new List<GetPortsReply.Types.Port>();
        foreach (var container in containers)
        {
            foreach (var containerPort in container.Ports)
            {
                if (containerPort.PublicPort != 0)
                {
                    GetPortsReply.Types.Port.Types.ProtocolType proto;
                    if (containerPort.Type.Equals("tcp"))
                        proto = GetPortsReply.Types.Port.Types.ProtocolType.Tcp;
                    else
                        proto = GetPortsReply.Types.Port.Types.ProtocolType.Udp;
                    
                    var port = new GetPortsReply.Types.Port()
                    {
                        Action = GetPortsReply.Types.Port.Types.ActionType.Accept,
                        Enable = true,
                        Position = 0,
                        Protocol = proto,
                        Type = GetPortsReply.Types.Port.Types.TypeType.In,
                        DestPort = containerPort.PublicPort
                        
                    };
                    ports.Add(port);
                }
                
            }
        }
        return await Task.FromResult(new GetPortsReply
        {
            Ports = { ports },
            Timestamp = DateTime.Now.ToTimestamp()
        });
    }
    
}