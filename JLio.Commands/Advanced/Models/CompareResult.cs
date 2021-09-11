using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JLio.Commands.Advanced.Models
{
    public class CompareResult
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("differenceSubType", NullValueHandling = NullValueHandling.Ignore)]
        public eDifferenceSubType DifferenceSubType { get; set; } = eDifferenceSubType.NotSet;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("differenceType", NullValueHandling = NullValueHandling.Ignore)]
        public DifferenceType DifferenceType { get; set; }

        [JsonProperty("firstPath", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstPath { get; set; }

        [JsonProperty("foundDifference")]
        public bool FoundDifference { get; set; }

        [JsonProperty("secondPath", NullValueHandling = NullValueHandling.Ignore)]
        public string SecondPath { get; set; }
    }
}