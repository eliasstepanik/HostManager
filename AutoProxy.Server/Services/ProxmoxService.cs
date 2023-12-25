using AutoProxy.Server.Docker;
using AutoProxy.Server.Models;
using AutoProxy.Server.Proxmox;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using GetPortsReply = AutoProxy.Server.Proxmox.GetPortsReply;
using GetPortsRequest = AutoProxy.Server.Proxmox.GetPortsRequest;
using Method = Google.Protobuf.WellKnownTypes.Method;
using Port = AutoProxy.Server.Models.Port;

namespace AutoProxy.Server.Services;

public class ProxmoxService(ILogger<ProxmoxService> logger) : Server.Proxmox.ProxmoxService.ProxmoxServiceBase
{
    private DockerClient _client = new DockerClientConfiguration()
        .CreateClient();
    private TicketRequest? _ticket;

    private async Task<TicketRequest?> GetTicket()
    {
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.118:8006"
        };
        foreach (var url in urls)
        {
            try
            {
                var options = new RestClientOptions(url)
                {
                    MaxTimeout = -1,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api2/json/access/ticket", RestSharp.Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("username", "portsync@pve");
                request.AddParameter("password", "8I*x5Y*gc101F+kS");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.Content != null)
                {
                    var ticket = TicketRequest.FromJson(response.Content);
                    return ticket;
                }
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    public override async Task<GetPortsReply> GetPorts(GetPortsRequest request, ServerCallContext context)
    {
        if (_ticket == null)
            _ticket = await GetTicket();
        
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.118:8006"
        };
        foreach (var url in urls)
        {
            try
            {

                var options = new RestClientOptions(url)
                {
                    MaxTimeout = -1,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };
                
                var client = new RestClient(options);
                var restRequest = new RestRequest($"/api2/json/cluster/firewall/groups/{request.Group}/");
                restRequest.AddHeader("Authorization", "PVEAuthCookie=" + _ticket!.Data.Ticket);
                restRequest.AddHeader("CSRFPreventionToken", _ticket!.Data.CsrfPreventionToken);
                RestResponse response = await client.ExecuteAsync(restRequest);
                if (response.Content != null)
                {
                    PortRequest? portRequest = JsonConvert.DeserializeObject<PortRequest>(response.Content);

                    var filterdPorts = new List<GetPortsReply.Types.Port>();
                    if (portRequest.Ports == null)
                    {
                        logger.LogError("Get Ports request Failed");
                        return await Task.FromResult(new GetPortsReply
                        {
                            Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
                        });
                    }

                    foreach (var portRequestPort in portRequest.Ports)
                    {
                        if (portRequestPort.Dport != 0)
                        {
                            GetPortsReply.Types.Port.Types.ProtocolType proto;
                            if (portRequestPort.Proto.ToLower().Equals("tcp"))
                                proto = GetPortsReply.Types.Port.Types.ProtocolType.Tcp;
                            else
                                proto = GetPortsReply.Types.Port.Types.ProtocolType.Udp;
                            
                            filterdPorts.Add(new GetPortsReply.Types.Port()
                            {
                                Action = GetPortsReply.Types.Port.Types.ActionType.Accept,
                                Enable = true,
                                Protocol = proto,
                                Type = GetPortsReply.Types.Port.Types.TypeType.In,
                                DestPort = int.Parse(portRequestPort.Dport.ToString()),
                                Postion = int.Parse(portRequestPort.Pos.ToString())
                                
                            });
                        }

                    }

                    logger.LogInformation("Sending Proxmox Ports to Client");
                    return await Task.FromResult(new GetPortsReply
                    {
                        Ports = { filterdPorts },
                        Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
                    });

                }

            }
            catch
            {
                // ignored
            }
        }
        
        logger.LogInformation("Sending Proxmox Ports to Client");
        return await Task.FromResult(new GetPortsReply
        {
            Timestamp = DateTime.Now.ToUniversalTime().ToTimestamp()
        });
    }

    public override async Task<OpenPortReply> OpenPort(OpenPortRequest request, ServerCallContext context)
    {
        if (_ticket == null)
            _ticket = await GetTicket();


        var port = new Port()
        {
            Dport = request.Port.DestPort,
            Proto = request.Port.Protocol.ToString().ToLower(),
        };
        

        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.118:8006"
        };
        foreach (var url in urls)
        {
            try
            {
                var options = new RestClientOptions(url)
                {
                    MaxTimeout = -1,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                };
                var client = new RestClient(options);
                var restRequest = new RestRequest($"/api2/json/cluster/firewall/groups/{request.Group}", RestSharp.Method.Post);
                restRequest.AddHeader("Authorization", "PVEAuthCookie=" + _ticket!.Data.Ticket);
                restRequest.AddHeader("CSRFPreventionToken", _ticket!.Data.CsrfPreventionToken);
                restRequest.AddHeader("Content-Type", "application/json");
                restRequest.AddStringBody(JsonConvert.SerializeObject(port), DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(restRequest);
                if (!response.IsSuccessful)
                {
                    logger.LogError("Add Ports request Failed!");
                    return await Task.FromResult(new OpenPortReply()
                    {
                        Success = false
                    });
                }
            }
            catch
            {
                // ignored
            }
            
        }
        
        logger.LogInformation("Port {Port} got opened", request.Port.DestPort);
        return await Task.FromResult(new OpenPortReply()
        {
            Success = true
        });
    }

    public override async Task<ClosePortReply> ClosePort(ClosePortRequest request, ServerCallContext context)
    {
        if (_ticket == null)
            _ticket = await GetTicket();
        
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.118:8006"
        };
        foreach (var url in urls)
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    var options = new RestClientOptions(url)
                    {
                        MaxTimeout = -1,
                        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    };
                    var client = new RestClient(options);
                    var restRequest = new RestRequest($"/api2/json/cluster/firewall/groups/{request.Group}/" + request.Position, RestSharp.Method.Delete);
                    restRequest.AddHeader("Authorization", "PVEAuthCookie=" + _ticket!.Data.Ticket);
                    restRequest.AddHeader("CSRFPreventionToken", _ticket!.Data.CsrfPreventionToken);

                    RestResponse response = await client.ExecuteAsync(restRequest);
                    if (response.IsSuccessful)
                    {
                        success = true;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        logger.LogInformation("Port {Port} got closed", request.Position);
        return await Task.FromResult(new ClosePortReply()
        {
            Success = true
        });
    }
}