using System.Collections.Generic;
using Newtonsoft.Json;

namespace JLio.Commands.Advanced
{
    public class MergeArraySettings
    {
        [JsonProperty("arrayPath")]
        public string ArrayPath { get; set; } = string.Empty;

        [JsonProperty("keyPaths")]
        public List<string> KeyPaths { get; set; } = new List<string>();

        [JsonProperty("uniqueItemsWithoutKeys")]
        public bool UniqueItemsWithoutKeys { get; set; }
    }
}