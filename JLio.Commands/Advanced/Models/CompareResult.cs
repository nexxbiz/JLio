using JLio.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Advanced.Models;

public class CompareResult
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("differenceSubType", NullValueHandling = NullValueHandling.Ignore)]
    public eDifferenceSubType DifferenceSubType { get; set; } = eDifferenceSubType.NotSet;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("differenceType", NullValueHandling = NullValueHandling.Ignore)]
    public DifferenceType DifferenceType { get; set; }

    [JsonProperty("firstPath", NullValueHandling = NullValueHandling.Ignore)]
    public string FirstPath { get; set; }

    [JsonProperty("foundDifference")]
    public bool FoundDifference { get; set; }

    [JsonProperty("secondPath", NullValueHandling = NullValueHandling.Ignore)]
    public string SecondPath { get; set; }

    public static CompareResult GetStructureDifferenceResult(JObject source, JObject target, string propertyName,
        IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.StructureDifference,
            DifferenceSubType = eDifferenceSubType.NotEquals,
            Description =
                $"The structure is different. Source: ({sourcePath}{fetcher.PathDelimiter}{propertyName}) -->  {source.ContainsKey(propertyName)} - Target:({targetPath}{fetcher.PathDelimiter}{propertyName}) -->  {target.ContainsKey(propertyName)}",
            FoundDifference = true
        };
    }

    public static CompareResult GetInBothArraysResult(JArray source, JArray target, string value,
        IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ArrayDifference,
            DifferenceSubType = eDifferenceSubType.Equals,
            Description =
                $"Both arrays contain Value:{value} . Source: ({sourcePath}) - Target:({targetPath})",
            FoundDifference = false
        };
    }

    public static CompareResult GetInBothArraysDifferentIndexResult(JArray source, int sourceIndex, JArray target,
        int targetIndex, string value, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ArrayDifference,
            DifferenceSubType = eDifferenceSubType.IndexDifference,
            Description =
                $"Index of items are Different . Source: ({sourcePath}[sourceIndex]) - Target:({targetPath}[targetIndex])",
            FoundDifference = true
        };
    }

    public static CompareResult GetIsInOneOfArraysResult(JArray source, bool inSource, JArray target, bool inTarget,
        string value, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ValueDifference,
            DifferenceSubType = eDifferenceSubType.NotEquals,
            Description =
                $"The content of the array is different. Source: ({sourcePath}) --> {inSource} - Target:({targetPath}) --> {inTarget}. Value:{value}",
            FoundDifference = true
        };
    }

    public static CompareResult GetDifferentCountOfArrayItemsResult(JArray source, JArray target,
        IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ValueDifference,
            DifferenceSubType = eDifferenceSubType.Equals,
            Description =
                $"The array's have different number of items. Source: ({sourcePath}): {source.Count}  - Target:({targetPath}): {target.Count} ",
            FoundDifference = false
        };
    }

    public static CompareResult GetSameCountOfArrayItemsResult(JArray source, JArray target, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ArrayDifference,
            DifferenceSubType = eDifferenceSubType.Equals,
            Description =
                $"Both array's have {source.Count} items. Source: ({sourcePath}) - Target:({targetPath})",
            FoundDifference = false
        };
    }

    public static CompareResult GetNumberValueDifferenceResult(JToken source, string sourceValue, JToken target,
        string targetValue, eDifferenceSubType subType, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ValueDifference,
            DifferenceSubType = subType,
            Description =
                $"The values are different {subType}. Source: ({sourcePath}) --> {sourceValue} - Target:({targetPath}) --> {targetValue}",
            FoundDifference = true
        };
    }

    public static CompareResult GetSameValueResult(JToken source, JToken target, string value,
        IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.NoDifference,
            DifferenceSubType = eDifferenceSubType.Equals,
            Description =
                $"The values are the same. Source: ({sourcePath}) - Target:({targetPath}) --> {value}",
            FoundDifference = false
        };
    }

    public static CompareResult GetDifferentValueResult(JToken source, string sourceValue, JToken target,
        string targetValue, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.ValueDifference,
            DifferenceSubType = eDifferenceSubType.NotEquals,
            Description =
                $"The values are different. Source: ({sourcePath}) --> {sourceValue} - Target:({targetPath}) --> {targetValue}",
            FoundDifference = true
        };
    }

    public static CompareResult GetDifferentTypeResult(JToken source, JToken target, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,

            DifferenceType = DifferenceType.TypeDifference,
            Description =
                $"The types are different. Source: ({sourcePath}) --> {source.Type} - Target:({targetPath}) --> {target.Type}",
            FoundDifference = true
        };
    }

    public static CompareResult GetSameTypeResult(JToken source, JToken target, IItemsFetcher fetcher)
    {
        var sourcePath = GetPath(source, fetcher);
        var targetPath = GetPath(target, fetcher);
        return new CompareResult
        {
            FirstPath = sourcePath,
            SecondPath = targetPath,
            DifferenceType = DifferenceType.TypeDifference,
            Description =
                $"The types are the same. Source: ({sourcePath}) --> {source.Type} - Target:({targetPath}) --> {target.Type}",
            FoundDifference = false
        };
    }

    private static string GetPath(JToken token, IItemsFetcher fetcher)
    {
        return $"{fetcher.RootPathIndicator}{fetcher.PathDelimiter}{token.Path}";
    }
}