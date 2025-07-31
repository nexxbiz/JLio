using JLio.Core;
using JLio.Core.Contracts;
using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public class ResolveValue
    {
        [JsonProperty("targetPath")]
        public string TargetPath { get; set; }

        [JsonProperty("value")]
        public IFunctionSupportedValue Value { get; set; }
    }
}