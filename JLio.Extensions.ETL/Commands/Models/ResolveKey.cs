using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class ResolveKey
    {
        [JsonProperty("keyPath")]
        public required string KeyPath { get; set; }

        [JsonProperty("referenceKeyPath")]
        public required string ReferenceKeyPath { get; set; }
    }
}