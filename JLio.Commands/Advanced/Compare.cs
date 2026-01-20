using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JLio.Commands.Advanced.Models;
using JLio.Commands.Advanced.Settings;
using JLio.Commands.Builders;
using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Advanced;

public class Compare : CommandBase
{
    private IExecutionContext executionContext;

    [JsonProperty("firstPath")]
    public string FirstPath { get; set; }

    [JsonProperty("resultPath")]
    public string ResultPath { get; set; }

    [JsonProperty("secondPath")]
    public string SecondPath { get; set; }

    [JsonProperty("settings")]
    public CompareSettings Settings { get; set; }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(FirstPath))
            result.ValidationMessages.Add($"FirstPath property for {CommandName} command is missing");

        if (string.IsNullOrWhiteSpace(SecondPath))
            result.ValidationMessages.Add($"SecondPath property for {CommandName} command is missing");

        if (string.IsNullOrWhiteSpace(ResultPath))
            result.ValidationMessages.Add($"ResultPath property for {CommandName} command is missing");

        if (Settings == null)
            result.ValidationMessages.Add($"Settings property for {CommandName} command is missing");

        return result;
    }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        executionContext = context;
        var compareResults = new CompareResults();
        var validationResult = ValidateCommandInstance();
        if (!validationResult.IsValid)
        {
            if (executionContext.Logger != null)
                validationResult.ValidationMessages.ForEach(i =>
                    executionContext.Logger.Log(LogLevel.Warning, CoreConstants.CommandExecution, i));

            return new JLioExecutionResult(false, dataContext);
        }

        foreach (var source in executionContext.ItemsFetcher.SelectTokens(FirstPath, dataContext))
        foreach (var target in executionContext.ItemsFetcher.SelectTokens(SecondPath, dataContext))
            compareResults.AddRange(CompareTokens(source, target));

        var filteredResults = FilterCompareResult(compareResults);
        SetResults(dataContext, JToken.FromObject(filteredResults));

        return new JLioExecutionResult(true, dataContext);
    }

    private CompareResults FilterCompareResult(CompareResults results)
    {
        if (Settings.ResultTypes == null || !Settings.ResultTypes.Any()) return results;

        var filtered = new List<CompareResult>();
        foreach (var r in results)
        {
            if (Settings.ResultTypes.Contains(r.DifferenceType))
            {
                filtered.Add(r);
            }
        }
        return new CompareResults(filtered);
    }

    private void SetResults(JToken dataContext, JToken compareResults)
    {
        var resultTargets = dataContext.SelectTokens(ResultPath);

        var enumerable = resultTargets as JToken[] ?? resultTargets.ToArray();

        var script = new JLioScript();
        if (!enumerable.Any())
        {
            script.Add(compareResults).OnPath(ResultPath).Execute(dataContext, executionContext);
            return;
        }

        foreach (var resultTarget in enumerable)
            script.Set(compareResults).OnPath(resultTarget.Path).Execute(dataContext, executionContext);
    }

    private CompareResults CompareTokens(JToken source, JToken target)
    {
        var result = new CompareResults();
        if (source.Type != target.Type)
        {
            result.Add(CompareResult.GetDifferentTypeResult(source, target, executionContext.ItemsFetcher));
        }
        else
        {
            result.Add(CompareResult.GetSameTypeResult(source, target, executionContext.ItemsFetcher));
            result.AddRange(CompareSameTypeObjects(source, target));
        }

        return result;
    }

    private CompareResults CompareSameTypeObjects(JToken source, JToken target)
    {
        var result = new CompareResults();
        switch (source.Type)
        {
            case JTokenType.Boolean:
                result.AddRange(CompareBoolean(source, target));
                break;
            case JTokenType.Array:
                result.AddRange(CompareArray((JArray) source, (JArray) target));
                break;
            case JTokenType.Float:
                result.AddRange(CompareIntegerNumbers(source, target));
                break;
            case JTokenType.Integer:
                result.AddRange(CompareFloatNumbers(source, target));
                break;
            case JTokenType.Object:
                result.AddRange(CompareObjectProperties((JObject) source, (JObject) target));
                break;
            default:
                result.AddRange(CompareDeepEquals(source, target));
                break;
        }

        return result;
    }

    private IEnumerable<CompareResult> CompareObjectProperties(JObject source, JObject target)
    {
        var result = new CompareResults();
        var allProperties = new List<string>();
        allProperties.AddRange(GetPropertyNames(source));
        allProperties.AddRange(GetPropertyNames(target));

        foreach (var property in allProperties.Distinct())
            result.AddRange(CompareProperty(source, target, property));
        return result;
    }

    private CompareResults CompareProperty(JObject source, JObject target, string propertyName)
    {
        var result = new CompareResults();
        var sourceToken = source[propertyName];
        var targetToken = target[propertyName];
        if (sourceToken == null || targetToken == null)
            result.Add(CompareResult.GetStructureDifferenceResult(source, target, propertyName,
                executionContext.ItemsFetcher));
        else
            result.AddRange(CompareTokens(sourceToken, targetToken));
        return result;
    }

    private static IEnumerable<string> GetPropertyNames(JObject source)
    {
        return source.Properties().ToList().Select(i => i.Name);
    }

    private CompareResults CompareArray(JArray source, JArray target)
    {
        var result = new CompareResults
        {
            source.Count == target.Count
                ? CompareResult.GetSameCountOfArrayItemsResult(source, target, executionContext.ItemsFetcher)
                : CompareResult.GetDifferentCountOfArrayItemsResult(source, target, executionContext.ItemsFetcher)
        };
        result.AddRange(CompareArrayItems(source, target));
        return result;
    }

    private CompareResults CompareArrayItems(JArray source, JArray target)
    {
        var foundTargetItems = new List<int>();

        var result = new CompareResults();
        foreach (var token in source.Children())
            result.AddRange(CompareArrayItem(source, target, token, foundTargetItems));
        for (var i = 0; i < target.Count; i++)
        {
            if (foundTargetItems.Contains(i)) continue;
            result.Add(CompareResult.GetIsInOneOfArraysResult(source, false, target, true,
                target[i].ToString(Formatting.None), executionContext.ItemsFetcher));
        }

        return result;
    }

    private CompareResults CompareArrayItem(JArray source, JArray target, JToken sourceItemToCompare,
        List<int> foundTargetIndexes)
    {
        var result = new CompareResults();
        var settings = Settings.ArraySettings.FirstOrDefault(i => i.ArrayPath == $"$.{source.Path}");

        var IsInTarget = ArrayHelpers.FindInArray(target, sourceItemToCompare, foundTargetIndexes, settings);

        if (IsInTarget.Found)
        {
            foundTargetIndexes.Add(IsInTarget.Index);
            HandleFoundInBothArrays(source, target, sourceItemToCompare, result, IsInTarget, settings);
        }
        else
        {
            result.Add(CompareResult.GetIsInOneOfArraysResult(source, true, target, false,
                sourceItemToCompare.ToString(Formatting.None), executionContext.ItemsFetcher));
        }

        return result;
    }

    private void HandleFoundInBothArrays(JArray source, JArray target, JToken sourceItemToCompare,
        CompareResults result,
        (bool Found, JToken Item, int Index) IsInTarget, CompareArraySettings settings)
    {
        if (sourceItemToCompare.Type == JTokenType.Object)
            result.AddRange(CompareTokens(sourceItemToCompare, IsInTarget.Item));
        result.Add(CompareResult.GetInBothArraysResult(source, target,
            sourceItemToCompare.ToString(Formatting.None), executionContext.ItemsFetcher));
        var sourceIndex = source.IndexOf(sourceItemToCompare);
        if (settings != null && settings.UniqueIndexMatching && IsInTarget.Index != sourceIndex)
            result.Add(CompareResult.GetInBothArraysDifferentIndexResult(source, sourceIndex, target,
                IsInTarget.Index,
                sourceItemToCompare.ToString(Formatting.None), executionContext.ItemsFetcher));
    }

    private CompareResults CompareIntegerNumbers(JToken source, JToken target)
    {
        var sourceValue = (long) source;
        var targetValue = (long) target;
        if (sourceValue == targetValue)
            return new CompareResults(CompareResult.GetSameValueResult(source, target,
                sourceValue.ToString(CultureInfo.InvariantCulture), executionContext.ItemsFetcher));

        if (sourceValue < targetValue)
            return new CompareResults(CompareResult.GetNumberValueDifferenceResult(source,
                sourceValue.ToString(CultureInfo.InvariantCulture), target,
                targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.LessThan,
                executionContext.ItemsFetcher));
        return new CompareResults(CompareResult.GetNumberValueDifferenceResult(source,
            sourceValue.ToString(CultureInfo.InvariantCulture), target,
            targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.GreaterThan,
            executionContext.ItemsFetcher));
    }

    private CompareResults CompareFloatNumbers(JToken source, JToken target)
    {
        var sourceValue = (double) source;
        var targetValue = (double) target;
        if (Math.Abs(sourceValue - targetValue) < 0.0000000001)
            return new CompareResults(CompareResult.GetSameValueResult(source, target,
                sourceValue.ToString(CultureInfo.InvariantCulture), executionContext.ItemsFetcher));

        if (sourceValue < targetValue)
            return new CompareResults(CompareResult.GetNumberValueDifferenceResult(source,
                sourceValue.ToString(CultureInfo.InvariantCulture), target,
                targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.LessThan,
                executionContext.ItemsFetcher));
        return new CompareResults(CompareResult.GetNumberValueDifferenceResult(source,
            sourceValue.ToString(CultureInfo.InvariantCulture), target,
            targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.GreaterThan,
            executionContext.ItemsFetcher));
    }

    private CompareResults CompareDeepEquals(JToken source, JToken target)
    {
        if (JToken.DeepEquals(source, target))
            return new CompareResults(CompareResult.GetSameValueResult(source, target,
                source.ToString(Formatting.None), executionContext.ItemsFetcher));

        return new CompareResults(CompareResult.GetDifferentValueResult(source, source.ToString(Formatting.None),
            target,
            target.ToString(Formatting.None), executionContext.ItemsFetcher));
    }

    private CompareResults CompareBoolean(JToken source, JToken target)
    {
        var sourceValue = (bool) source;
        var targetValue = (bool) target;

        if (sourceValue == targetValue)
            return new CompareResults(CompareResult.GetSameValueResult(source, target,
                source.ToString(Formatting.None), executionContext.ItemsFetcher));

        return new CompareResults(CompareResult.GetDifferentValueResult(source, sourceValue.ToString(), target,
            targetValue.ToString(), executionContext.ItemsFetcher));
    }
}