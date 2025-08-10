using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class FlattenMetadata
    {
        [JsonProperty("originalStructure")]
        public Dictionary<string, string> OriginalStructure { get; set; } = new();

        [JsonProperty("delimiter")]
        public string? Delimiter { get; set; }

        [JsonProperty("arrayDelimiter")]
        public string? ArrayDelimiter { get; set; }

        [JsonProperty("includeArrayIndices")]
        public bool IncludeArrayIndices { get; set; }

        [JsonProperty("preserveTypes")]
        public bool PreserveTypes { get; set; }

        [JsonProperty("typeIndicator")]
        public string? TypeIndicator { get; set; }

        [JsonProperty("timestamp")]
        public string? Timestamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("rootPath")]
        public string? RootPath { get; set; }

        [JsonProperty("metadataKey")]
        public string? MetadataKey { get; set; }
    }
}