using AutoProxy.Server;
using Grpc.Net.Client;

namespace PortUpdate.Services;

public class PortClient
{
    public PortClient()
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7280");
        var client = new DockerService.DockerServiceClient(channel);
        var reply = client.GetPorts();
    }

    public void Update()
    {
        
    }
}