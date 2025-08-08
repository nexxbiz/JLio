using Newtonsoft.Json;
using System.Collections.Generic;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class RestoreSettings
    {
        [JsonProperty("metadataPath")]
        public string MetadataPath { get; set; } = "";

        [JsonProperty("metadataKey")]
        public string MetadataKey { get; set; } = "_flattenMetadata";

        [JsonProperty("removeMetadata")]
        public bool RemoveMetadata { get; set; } = true;

        [JsonProperty("useJsonPathColumn")]
        public bool UseJsonPathColumn { get; set; } = false;

        [JsonProperty("jsonPathColumn")]
        public string JsonPathColumn { get; set; } = "_jsonpath";

        [JsonProperty("delimiter")]
        public string Delimiter { get; set; } = ".";

        [JsonProperty("arrayDelimiter")]
        public string ArrayDelimiter { get; set; } = ".";

        [JsonProperty("strictMode")]
        public bool StrictMode { get; set; } = false; // If true, fails on missing metadata
    }
}