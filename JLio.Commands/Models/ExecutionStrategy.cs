using Newtonsoft.Json;

namespace JLio.Commands.Models;

public class ExecutionStrategy
{
    [JsonProperty("mode")]
    public string Mode { get; set; } // "firstMatch", "allMatches", "bestMatch"

    [JsonProperty("conflictResolution")]
    public string ConflictResolution { get; set; } // "priority", "lastWins", "merge"

    [JsonProperty("stopOnError")]
    public bool StopOnError { get; set; }
}