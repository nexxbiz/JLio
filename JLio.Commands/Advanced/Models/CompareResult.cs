using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Advanced.Models
{
    public class CompareResult
    {
        [JsonProperty("arrayFilter", NullValueHandling = NullValueHandling.Ignore)]
        public string ArrayFilter { get; set; }

        [JsonProperty("compareValue", NullValueHandling = NullValueHandling.Ignore)]
        public JToken CompareValue { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public eDifferenceSubType DifferenceSubType { get; set; } = eDifferenceSubType.NotSet;

        [JsonProperty("differenceType", NullValueHandling = NullValueHandling.Ignore)]
        public DifferenceType DifferenceType { get; set; }

        [JsonProperty("firstPath", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstPath { get; set; }

        [JsonProperty("isDifference")]
        public bool IsDifference { get; set; }

        [JsonProperty("secondPath", NullValueHandling = NullValueHandling.Ignore)]
        public string SecondPath { get; set; }
    }
}