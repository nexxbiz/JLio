using Newtonsoft.Json;

namespace JLio.Commands.Models;

public class DecisionInput
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("transform")]
    public string Transform { get; set; }
}
