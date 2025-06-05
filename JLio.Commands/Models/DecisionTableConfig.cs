using JLio.Core.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JLio.Commands.Models;

// Supporting classes for JSON deserialization
public class DecisionTableConfig
{
    [JsonProperty("inputs")]
    public List<DecisionInput> Inputs { get; set; }

    [JsonProperty("outputs")]
    public List<DecisionOutput> Outputs { get; set; }

    [JsonProperty("rules")]
    public List<DecisionRule> Rules { get; set; }

    [JsonProperty("defaultResults")]
    public Dictionary<string, IFunctionSupportedValue> DefaultResults { get; set; }

    [JsonProperty("executionStrategy")]
    public ExecutionStrategy ExecutionStrategy { get; set; }
}
