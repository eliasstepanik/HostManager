using AutoProxy.Server.Docker;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace AutoProxy.Server.Services;

public class DockerService(ILogger<DockerService> logger) : Server.Docker.DockerService.DockerServiceBase
{

    public override async Task<GetPortsReply> GetPorts(GetPortsRequest request, ServerCallContext context)
    {
        var client = new DockerClientConfiguration()
            .CreateClient();
        var containers = new List<ContainerListResponse>();
        try
        {
            if (client == null)
                throw new Exception("No Docker Client!");
            
            var listContainersAsync = await client.Containers.ListContainersAsync(new ContainersListParameters());
            containers.AddRange(listContainersAsync);
        }
        catch (Exception e)
        {
            await Task.FromResult(new GetPortsReply()
            {
                Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
            });
        }


        var ports = new List<DockerPort>();
        foreach (var container in containers)
        {
            foreach (var containerPort in container.Ports)
            {
                if (containerPort.PublicPort != 0)
                {
                    DockerPort.Types.ProtocolType proto;
                    if (containerPort.Type.Equals("tcp"))
                        proto = DockerPort.Types.ProtocolType.Tcp;
                    else
                        proto = DockerPort.Types.ProtocolType.Udp;

                    var port = new DockerPort()
                    {
                        Action = DockerPort.Types.ActionType.Accept,
                        Enable = true,
                        Protocol = proto,
                        Type = DockerPort.Types.TypeType.In,
                        DestPort = containerPort.PublicPort

                    };
                    ports.Add(port);
                }

            }
        }
        
        logger.LogInformation("Sending Docker Ports to Client");
        client?.Dispose();
        return await Task.FromResult(new GetPortsReply
        {
            Ports = { ports },
            Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
        });
        
    }
    public override async Task<GetDomainsReply> GetDomains(GetDomainsRequest request, ServerCallContext context)
    {
        var client = new DockerClientConfiguration()
            .CreateClient();
        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
        var domains = new List<GetDomainsReply.Types.Domain>();

        foreach (var container in containers)
        {
            foreach (var (key, value) in container.Labels)
            {
                if (key.Contains("caddy"))
                    domains.AddRange(await FilterDomains(container));
            }
        }
        
        
        logger.LogInformation("Sending Docker Domains to Client");
        client.Dispose();
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