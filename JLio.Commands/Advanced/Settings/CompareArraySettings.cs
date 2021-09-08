using System.Collections.Generic;
using Newtonsoft.Json;

namespace JLio.Commands.Advanced.Settings
{
    public class CompareArraySettings
    {
        [JsonProperty("arrayPath")]
        public string ArrayPath { get; set; } = string.Empty;

        [JsonProperty("keyPaths")]
        public List<string> KeyPaths { get; set; } = new List<string>();

        [JsonProperty("uniqueIndexMatching")]
        public bool UniqueIndexMatching { get; set; }
    }
}