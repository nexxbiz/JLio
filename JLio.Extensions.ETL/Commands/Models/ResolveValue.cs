using JLio.Core;
using JLio.Core.Contracts;
using Newtonsoft.Json;

namespace JLio.Extensions.ETL.Commands.Models
{
    public enum ResolveTypeBehavior
    {
        /// <summary>
        /// Always returns an array, empty when no items are found
        /// </summary>
        AlwaysAsArray,
        
        /// <summary>
        /// Depending on the number of items: 1 -> object, multiple -> array, none -> null
        /// </summary>
        DependingOnResult,
        
        /// <summary>
        /// Always returns as a single object, throws error if multiple items are found
        /// </summary>
        AlwaysAsObject
    }

    public class ResolveValue
    {
        [JsonProperty("targetPath")]
        public required string TargetPath { get; set; }

        [JsonProperty("value")]
        public required IFunctionSupportedValue Value { get; set; }

        [JsonProperty("resolveTypeBehavior")]
        public ResolveTypeBehavior ResolveTypeBehavior { get; set; } = ResolveTypeBehavior.DependingOnResult;
    }
}