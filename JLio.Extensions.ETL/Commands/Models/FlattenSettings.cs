using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class FlattenSettings
    {
        [JsonProperty("delimiter")]
        public string Delimiter { get; set; } = ".";

        [JsonProperty("arrayDelimiter")]
        public string ArrayDelimiter { get; set; } = ".";

        [JsonProperty("includeArrayIndices")]
        public bool IncludeArrayIndices { get; set; } = true;

        [JsonProperty("includeJsonPath")]
        public bool IncludeJsonPath { get; set; } = false;

        [JsonProperty("jsonPathColumn")]
        public string JsonPathColumn { get; set; } = "_jsonpath";

        [JsonProperty("metadataPath")]
        public string MetadataPath { get; set; } = "$";

        [JsonProperty("metadataKey")]
        public string MetadataKey { get; set; } = "_flattenMetadata";

        [JsonProperty("preserveTypes")]
        public bool PreserveTypes { get; set; } = true;

        [JsonProperty("typeIndicator")]
        public string TypeIndicator { get; set; } = "_type";

        [JsonProperty("maxDepth")]
        public int MaxDepth { get; set; } = -1; // -1 means unlimited

        [JsonProperty("excludePaths")]
        public List<string> ExcludePaths { get; set; } = new();

        [JsonProperty("includePaths")]
        public List<string> IncludePaths { get; set; } = new();
    }
}