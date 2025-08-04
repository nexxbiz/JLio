using Newtonsoft.Json;
using System.Collections.Generic;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class ResolveSetting
    {
        [JsonProperty("resolveKeys")]
        public List<ResolveKey> ResolveKeys { get; set; } = new List<ResolveKey>();

        [JsonProperty("referencesCollectionPath")]
        public string ReferencesCollectionPath { get; set; }

        [JsonProperty("values")]
        public List<ResolveValue> Values { get; set; } = new List<ResolveValue>();
    }
}