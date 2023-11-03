using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortUpdate.Models;
using RestSharp;

namespace PortUpdate.Services;

public class ProxmoxService
{

    private readonly ILogger<ProxmoxService> _logger;


    public ProxmoxService(ILogger<ProxmoxService> logger)
    {
        _logger = logger;
    }


    public async Task<TicketRequest?> GetTicket()
    {
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.66:8006"
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
                var request = new RestRequest("/api2/json/access/ticket", Method.Post);
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

    public async Task<PortRequest?> GetPorts(TicketRequest ticket)
    {
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.66:8006"
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
                var request = new RestRequest("/api2/json/cluster/firewall/groups/dynamic_public_ha/");
                request.AddHeader("Authorization", "PVEAuthCookie=" + ticket.Data.Ticket);
                request.AddHeader("CSRFPreventionToken", ticket.Data.CsrfPreventionToken);
                RestResponse response = await client.ExecuteAsync(request);
                if (response.Content != null)
                {
                    PortRequest? portRequest = JsonConvert.DeserializeObject<PortRequest>(response.Content);

                    var filterdPorts = new PortRequest();
                    filterdPorts.Ports = new List<Port>();
                    if (portRequest.Ports == null)
                    {
                        _logger.LogError("Get Ports request Failed");
                        return null;
                    }

                    foreach (var portRequestPort in portRequest.Ports)
                    {
                        if (portRequestPort.Dport != 0 && portRequestPort.Dport != 80 && portRequestPort.Dport != 443)
                        {
                            filterdPorts.Ports.Add(portRequestPort);
                        }

                    }

                    return filterdPorts;

                }

            }
            catch
            {
                // ignored
            }
        }
        return null;
    }

    public async void AddPort(Port port, TicketRequest ticket)
    {
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.66:8006"
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
                var request = new RestRequest("/api2/json/cluster/firewall/groups/dynamic_public_ha", Method.Post);
                request.AddHeader("Authorization", "PVEAuthCookie=" + ticket.Data.Ticket);
                request.AddHeader("CSRFPreventionToken", ticket.Data.CsrfPreventionToken);
                request.AddHeader("Content-Type", "application/json");
                request.AddStringBody(JsonConvert.SerializeObject(port), DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    _logger.LogError("Add Ports request Failed!");
                }
            }
            catch
            {
                // ignored
            }
            
        }
    }

    public async void RemovePort(long pos, TicketRequest ticket)
    {
        //TODO: Move to environment
        var urls = new string[]
        {
            "https://192.168.188.40:8006",
            "https://192.168.178.66:8006"
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
                var request = new RestRequest("/api2/json/cluster/firewall/groups/dynamic_public_ha/" + pos, Method.Delete);
                request.AddHeader("Authorization", "PVEAuthCookie=" + ticket.Data.Ticket);
                request.AddHeader("CSRFPreventionToken", ticket.Data.CsrfPreventionToken);

                RestResponse response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    _logger.LogError("Delete Port " + pos + " Request Failed");
                }
            }
            catch
            {
                // ignored
            }
        }
    }

}