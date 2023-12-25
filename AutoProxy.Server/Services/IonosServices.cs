using AutoProxy.Server.Ionos;
using AutoProxy.Server.Models;
using Docker.DotNet;
using Grpc.Core;
using Newtonsoft.Json;
using RestSharp;

namespace AutoProxy.Server.Services;

public class IonosServices(ILogger<IonosServices> logger) : Ionos.IonosService.IonosServiceBase
{
    private readonly ILogger<IonosServices> _logger = logger;
    private DockerClient _client = new DockerClientConfiguration()
        .CreateClient();


    public override async Task<UpdateDomainsReply> UpdateDomains(UpdateDomainsRequest request, ServerCallContext context)
    {
        var client = new RestClient(request.UpdateUrl);
        var restRequest = new RestRequest("",Method.Get);
        restRequest.AddHeader("Cookie", "0b04270753322c986927738ac2b6c0d8=ea099cbd8a6109c688f9831d6bbfa7a1; 5b66c83e4535f5f6bef8295496cfe559=e85228fccae97f107478bf9ef664e4eb; DPX=v1:ghOJrOzFTj:htgOaKFW:63d3bf8f:de");
        var body = @"";
        restRequest.AddParameter("text/plain", body,  ParameterType.RequestBody);

        try
        {
            _logger.LogInformation("Sending Update to Ionos");
            var response = await client.ExecuteAsync(restRequest);
            return new UpdateDomainsReply() { StatusCode = 200 };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new UpdateDomainsReply() { StatusCode = 443 };
        }
    }

    public override async Task<GetUpdateUrlReply> GetUpdateUrl(GetUpdateUrlRequest request, ServerCallContext context)
    {
        GetUpdateUrlReply reply = new();


        var domains = new List<string>();
        foreach (var c in request.Domains)
        {
            domains.Add(c);
        }
        
        var dyndns = new DynamicDnsRequest()
        {
            Domains = domains,
            Description = "My DynamicDns"
        };
        var content = JsonConvert.SerializeObject(dyndns);
        var client = new RestClient("https://api.hosting.ionos.com/dns/v1");
        var restRequest = new RestRequest("/dyndns", Method.Post);
        
        
        restRequest.AddHeader("X-API-Key", request.Key);
        
        restRequest.AddStringBody(content, ContentType.Json);
            
        
        try
        {
            var response =  client.ExecutePost<DynamicDnsResponse>(restRequest);
            reply.UpdateUrl = response.Content;
        }
        catch (Exception error)
        {
            _logger.LogError(error.Message);
            return null;
        }

        return reply;
    }
}