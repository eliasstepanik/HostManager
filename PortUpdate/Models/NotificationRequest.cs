namespace PortUpdate.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Action
{
    public string action { get; set; }
    public string label { get; set; }
    public string url { get; set; }
}

public class NotificationRequest
{
    public string topic { get; set; }
    public string message { get; set; }
    public string title { get; set; }
    public List<string> tags { get; set; }
    public int priority { get; set; }
    public string attach { get; set; }
    public string filename { get; set; }
    public string click { get; set; }
    public List<Action> actions { get; set; }
}

