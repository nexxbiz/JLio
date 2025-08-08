using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class CsvSettings
    {
        [JsonProperty("delimiter")]
        public string Delimiter { get; set; } = ",";

        [JsonProperty("includeHeaders")]
        public bool IncludeHeaders { get; set; } = true;

        [JsonProperty("includeTypeColumns")]
        public bool IncludeTypeColumns { get; set; } = false;

        [JsonProperty("typeColumnSuffix")]
        public string TypeColumnSuffix { get; set; } = "_type";

        [JsonProperty("quoteAllFields")]
        public bool QuoteAllFields { get; set; } = false;

        [JsonProperty("escapeQuoteChar")]
        public string EscapeQuoteChar { get; set; } = "\"";

        [JsonProperty("lineEnding")]
        public string LineEnding { get; set; } = "\r\n";

        [JsonProperty("encoding")]
        public string Encoding { get; set; } = "UTF-8";

        [JsonProperty("includeMetadata")]
        public bool IncludeMetadata { get; set; } = false;

        [JsonProperty("nullValueRepresentation")]
        public string NullValueRepresentation { get; set; } = "";

        [JsonProperty("booleanFormat")]
        public string BooleanFormat { get; set; } = "true,false"; // Format: "true,false" or "1,0" etc.
    }
}