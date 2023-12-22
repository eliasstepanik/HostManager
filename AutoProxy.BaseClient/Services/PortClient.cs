using System.Text.Json;
using AutoProxy.Server.Docker;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace AutoProxy.BaseClient.Services;

public class PortClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<PortClient> _logger;
    public PortClient(ILogger<PortClient> logger)
    {
        _logger = logger;
        _channel = GrpcChannel.ForAddress("http://localhost:5005");
    }

    public void GetPorts()
    {
        var client = new DockerService.DockerServiceClient(_channel);
        var reply = client.GetPorts(new GetPortsRequest());
        
        _logger.LogInformation("Got Ports {S}", JsonSerializer.Serialize(reply.Ports));
    }
}