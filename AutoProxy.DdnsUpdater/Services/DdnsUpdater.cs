using AutoProxy.Server.Docker;
using AutoProxy.Server.Ionos;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace AutoProxy.DdnsUpdater.Services;

public class DdnsUpdater(ILogger<DdnsUpdater> logger,IServiceScopeFactory factory, DdnsDbContext dbContext)
{
    private readonly ILogger<DdnsUpdater> _logger = logger;
    private readonly IServiceScopeFactory _factory = factory;
    private readonly DdnsDbContext _dbContext = dbContext;
    private readonly GrpcChannel _channel = GrpcChannel.ForAddress("http://hostmanager-server-1");
    
    private List<string>? UpdateUrLs { get; set; }


    public async void Start()
    {
        _logger.LogInformation("Fetching UpdateURLs");
        UpdateUrLs = await GetUpdateUrLs();
        while (UpdateUrLs == null || UpdateUrLs.Count == 0 )
        {
            _logger.LogInformation($"Fetching UpdateURLs again.");
            UpdateUrLs = await GetUpdateUrLs();
        }
        
        _logger.LogInformation("Fetched {Count} UpdateURLs", UpdateUrLs.Count);
    }
    
    public async void Update()
    {
        var ionosClient = new IonosService.IonosServiceClient(_channel);
        if (UpdateUrLs != null)
        {
            foreach (var updateUrl in UpdateUrLs)
            {
                ionosClient.UpdateDomains(new UpdateDomainsRequest()
                {
                    UpdateUrl = updateUrl
                });
            }
        }
            
    }
    
    public async void SetUpdateUrl()
    {
        UpdateUrLs = await GetUpdateUrLs();
    }

    
    private async Task<List<string>> GetUpdateUrLs()
    {
        var dockerClient = new DockerService.DockerServiceClient(_channel);
        var ionosClient = new IonosService.IonosServiceClient(_channel);
        
        List<string> updateUrLs = new List<string>();

        var domains = dockerClient.GetDomains(new GetDomainsRequest());
        var list = domains.Domains.ToList();

        var dict = ToDomainDict(list);
        
        
        foreach (var domainList in dict)
        {
            var reply = ionosClient.GetUpdateUrl(new GetUpdateUrlRequest()
            {
                Domains = { domainList.Value },
                Key = domainList.Key
            });
            
            updateUrLs.Add(reply.UpdateUrl);
        }

        return updateUrLs;
    }


    private Dictionary<string, List<string>> ToDomainDict(List<GetDomainsReply.Types.Domain> domains)
    {
        var dict = new Dictionary<string, List<string>>();

        foreach (var domain in domains)
        {
            if (dict.ContainsKey(domain.ApiKey))
            {
                dict[domain.ApiKey].Add(domain.DomainName);
            }
            else
            {
                dict.Add(domain.ApiKey, new List<string>());
                dict[domain.ApiKey].Add(domain.DomainName);
            }
        }

        return dict;
    }
    
}


