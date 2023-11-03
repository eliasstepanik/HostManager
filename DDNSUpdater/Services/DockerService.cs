using DDNSUpdater.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DDNSUpdater.Services;

public class DockerService
{
    private Timer timer;
    private readonly ILogger<TimerService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly DockerClient _dockerClient;
    private readonly DataContext _context;
    private readonly int intervalMinutes;

    public DockerService(ILogger<TimerService> logger,IServiceScopeFactory factory, DockerClient dockerClient, DataContext context)
    {
        _logger = logger;
        _factory = factory;
        _dockerClient = dockerClient;
        _context = context;
    }

    public async Task<bool> UpdateDomainList()
    {
        var changed = false;
              
          
        var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        var domains = await _context.Domains.ToListAsync();
        
        
        
        
        foreach (var container in containers)
        {
            foreach (var (key, value) in container.Labels)
            {
                if (key.Contains("caddy"))
                {
                    domains = await _context.Domains.ToListAsync();
                    //var domainsFound = container.Labels["caddy"].Split(" ");
                    //var apiKey = container.Labels["caddy.tls.dns"].Replace("ionos ", "");
                    var domainsFound = await GetDomains(container);
                
                    foreach (var domain in domainsFound)
                    {
                        //Domain? find = domains.Find(d => d.DomainString.Equals(domain.DomainString));
                        var exists = false;
                        foreach (var d in domains)
                        {
                            if (d.DomainString.Equals(domain.DomainString))
                            {
                                exists = true;
                            }
                        }

                        if (!exists)
                        {
                            _logger.LogInformation("Adding Domain: " + domain.DomainString);
                            changed = true;
                            await _context.Domains.AddAsync(domain);
                            await _context.SaveChangesAsync(); 
                        }
                    }

                    await _context.SaveChangesAsync(); 
                }
            }

        }
        domains = await _context.Domains.ToListAsync();
        foreach (var domain in domains)
        {
            var found = false;
            
            foreach (var containerListResponse in containers)
            {
                var domainsInLabels = await GetDomains(containerListResponse);
                foreach (var domainInContainer in domainsInLabels)
                {
                    if (domainInContainer.DomainString.Equals(domain.DomainString))
                    {
                        found = true;
                    }
                }


                /*if (!containerListResponse.Labels.Contains(
                        new KeyValuePair<string, string>("caddy", domain.DomainString)))
                {
                    found = true;
                }*/
            }

            if (!found)
            {
                _context.Domains.Remove(domain);
            }
        }
        
        
        await _context.SaveChangesAsync();


        return changed;

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


    private async Task<List<Domain>> GetDomains(ContainerListResponse container)
    {
        var domains = new List<Domain>();
        var labels = container.Labels;


        var dnsRange = GetDnsRange(labels);
        
        foreach (var range in dnsRange)
        {
            foreach (var domainLabel in labels[range].Split(" "))
            {
                var domain = new Domain();
                domain.DomainString = domainLabel;
                domain.Key = labels[range + ".tls.dns"];
                domains.Add(domain);
            }
        }
        return domains;
    }
    
    
}