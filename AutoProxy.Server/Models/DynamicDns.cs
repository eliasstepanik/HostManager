using Newtonsoft.Json;

namespace AutoProxy.Server.Models;

public class DynamicDnsRequest
{
    [JsonProperty("domains")]
    public List<string> Domains { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }
}

public class DynamicDnsResponse
{
    public string BulkId { get; set; }
    public string UpdateUrl { get; set; }
    public List<string> Domains { get; set; }
    public string Description { get; set; }
}