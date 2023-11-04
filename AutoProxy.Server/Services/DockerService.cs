using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace AutoProxy.Server.Services;

public class DockerService : Server.DockerService.DockerServiceBase
{
    private readonly ILogger<DockerService> _logger;
    private DockerClient _client;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _client = new DockerClientConfiguration()
            .CreateClient();

    }

    public override async Task GetPorts(IAsyncStreamReader<GetPortsRequest> requestStream, IServerStreamWriter<GetPortsReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Port Client Connected!");
        // Read requests in a background task.
        var readTask = Task.Run(async () =>
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                if (message.ExitCode != 0)
                {
                    switch (message.ExitCode)
                    {
                        case 200:
                            _logger.LogInformation("Port Stream disconnected with Error Code 200");
                            return;
                        case 443:
                            _logger.LogError("Port Stream lost the Connection. Check the connectivity!");
                            return;
                    }
                    
                    _logger.LogError("The Port Stream exited with the unknown exitcode {MessageExitCode}", message.ExitCode);
                    return;
                }
                
                
            }
        });
        
        while (!readTask.IsCompleted)
        {
            _client = new DockerClientConfiguration()
                .CreateClient();
            var containers = new List<ContainerListResponse>();
            try
            {
                if (_client == null)
                    throw new Exception("No Docker Client!");
                
                var listContainersAsync = await _client.Containers.ListContainersAsync(new ContainersListParameters());
                containers.AddRange(listContainersAsync);
            }
            catch (Exception e)
            {
                await responseStream.WriteAsync(new ()
                {
                    Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
                });
            }


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
                            Protocol = proto,
                            Type = GetPortsReply.Types.Port.Types.TypeType.In,
                            DestPort = containerPort.PublicPort

                        };
                        ports.Add(port);
                    }

                }
            }
            
            
            await responseStream.WriteAsync(new ()
            {
                Ports = { ports },
                Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
            });
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
    }

    /*public override async Task<GetPortsReply> GetPorts(GetPortsRequest request, ServerCallContext context)
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
    }*/
    public override async Task<GetDomainsReply> GetDomains(GetDomainsRequest request, ServerCallContext context)
    {
        var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters());
        var domains = new List<GetDomainsReply.Types.Domain>();

        foreach (var container in containers)
        {
            foreach (var (key, value) in container.Labels)
            {
                if (key.Contains("caddy"))
                    domains.AddRange(await FilterDomains(container));
            }
        }
        
        
        return await Task.FromResult(new GetDomainsReply()
        {
            Domains = { domains }
        });
    }
    
    
    
    private List<string> GetDnsRange(IDictionary<string,string> labels)
    {
        var dnsRange = new List<string>();
        foreach (var keyValuePair in labels)
        {
            if (keyValuePair.Key.Contains("caddy"))
            {
                if (!keyValuePair.Key.Contains("reverse_proxy") && !keyValuePair.Key.Contains("tls.dns"))
                {
                    if (labels.ContainsKey($"{keyValuePair.Key}.tls.dns"))
                    {
                        dnsRange.Add(keyValuePair.Key);
                    }
                }
            }
        }

        return dnsRange;
    }
    private Task<List<GetDomainsReply.Types.Domain>> FilterDomains(ContainerListResponse container)
    {
        var domains = new List<GetDomainsReply.Types.Domain>();
        var labels = container.Labels;


        var dnsRange = GetDnsRange(labels);
        
        foreach (var range in dnsRange)
        {
            foreach (var domainLabel in labels[range].Split(" "))
            {
                var domain = new GetDomainsReply.Types.Domain();
                domain.DomainName = domainLabel;
                domain.ApiKey = labels[range + ".tls.dns"];
                domains.Add(domain);
            }
        }
        return Task.FromResult(domains);
    }
}