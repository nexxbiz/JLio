using Newtonsoft.Json;
using System.Collections.Generic;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class ResolveSetting
    {
        [JsonProperty("resolveKeys")]
        public List<ResolveKey> ResolveKeys { get; set; } = new ();

        [JsonProperty("referencesCollectionPath")]
        public required string ReferencesCollectionPath { get; set; }

        [JsonProperty("values")]
        public List<ResolveValue> Values { get; set; } = new ();
    }
}