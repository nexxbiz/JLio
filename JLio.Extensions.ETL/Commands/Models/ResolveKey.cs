using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class ResolveKey
    {
        [JsonProperty("keyPath")]
        public string KeyPath { get; set; }

        [JsonProperty("referenceKeyPath")]
        public string ReferenceKeyPath { get; set; }
    }
}