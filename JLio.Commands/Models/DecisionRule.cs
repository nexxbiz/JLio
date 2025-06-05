using JLio.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JLio.Commands.Models;

public class DecisionRule
{
    [JsonProperty("conditions")]

    public Dictionary<string, JToken> Conditions { get; set; }

    [JsonProperty("results")]
    public Dictionary<string, IFunctionSupportedValue> Results { get; set; }

    [JsonProperty("priority")]
    public int Priority { get; set; }
}
