using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AutoProxy.Server.Models;


public partial class PortRequest
{
    [JsonProperty("data")]
    public virtual List<Port> Ports { get; set; }
}

public partial class Port
{
    [JsonProperty("type")] 
    public virtual string Type { get; set; } = "in";

    [JsonProperty("log")]
    public virtual string Log { get; set; }

    [JsonProperty("dport")]
    /*[JsonConverter(typeof(StringConverter))]*/
    public virtual long Dport { get; set; }

    [JsonProperty("digest")]
    public virtual string Digest { get; set; }

    [JsonProperty("action")]
    public virtual string Action { get; set; } = "ACCEPT";

    [JsonProperty("enable")]
    public virtual long Enable { get; set; } = 1;

    [JsonProperty("pos")]
    public virtual long Pos { get; set; }

    [JsonProperty("proto")]
    public virtual string Proto { get; set; }
}
