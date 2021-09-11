﻿using System;
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

namespace JLio.Commands.Advanced
{
    public class Compare : CommandBase
    {
        private IExecutionOptions options;

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
            var result = new ValidationResult {IsValid = true};
            if (string.IsNullOrWhiteSpace(FirstPath))
            {
                result.ValidationMessages.Add($"FirstPath property for {CommandName} command is missing");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(SecondPath))
            {
                result.ValidationMessages.Add($"SecondPath property for {CommandName} command is missing");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(ResultPath))
            {
                result.ValidationMessages.Add($"ResultPath property for {CommandName} command is missing");
                result.IsValid = false;
            }

            if (Settings == null)
            {
                result.ValidationMessages.Add($"Settings property for {CommandName} command is missing");
                result.IsValid = false;
            }

            return result;
        }

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionOptions executionOptions)
        {
            options = executionOptions;
            var compareResults = new CompareResults();
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    options.Logger?.Log(LogLevel.Warning, Constants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            foreach (var source in options.ItemsFetcher.SelectTokens(FirstPath, dataContext))
            foreach (var target in options.ItemsFetcher.SelectTokens(SecondPath, dataContext))
                compareResults.AddRange(CompareTokens(source, target));
            var filteredResults = FilterCompareResult(compareResults);
            SetResults(dataContext, JToken.FromObject(filteredResults));

            return new JLioExecutionResult(true, dataContext);
        }

        private CompareResults FilterCompareResult(CompareResults results)
        {
            if (Settings.ResultTypes == null || !Settings.ResultTypes.Any()) return results;

            return new CompareResults(results.Where(r => Settings.ResultTypes.Contains(r.DifferenceType)).ToList());
        }

        private void SetResults(JToken dataContext, JToken compareResults)
        {
            var resultTargets = dataContext.SelectTokens(ResultPath);

            var enumerable = resultTargets as JToken[] ?? resultTargets.ToArray();

            var script = new JLioScript();
            if (!enumerable.Any())
            {
                script.Add(compareResults).OnPath(ResultPath).Execute(dataContext, options);
                return;
            }

            foreach (var resultTarget in enumerable)
                script.Set(compareResults).OnPath(resultTarget.Path).Execute(dataContext, options);
        }

        private CompareResults CompareTokens(JToken source, JToken target)
        {
            var result = new CompareResults();
            if (source.Type != target.Type)
            {
                result.Add(GetDifferentTypeResult(source, target));
            }
            else
            {
                result.Add(GetSameTypeResult(source, target));
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
                result.Add(GetStructureDifferenceResult(source, target, propertyName));
            else
                result.AddRange(CompareTokens(sourceToken, targetToken));
            return result;
        }

        private CompareResult GetStructureDifferenceResult(JObject source, JObject target, string propertyName)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
            return new CompareResult
            {
                FirstPath = sourcePath,
                SecondPath = targetPath,
                DifferenceType = DifferenceType.StructureDifference,
                DifferenceSubType = eDifferenceSubType.NotEquals,
                Description =
                    $"The structure is different. Source: ({sourcePath}{options.ItemsFetcher.PathDelimiter}{propertyName}) -->  {source.ContainsKey(propertyName)} - Target:({targetPath}{options.ItemsFetcher.PathDelimiter}{propertyName}) -->  {target.ContainsKey(propertyName)}",
                FoundDifference = true
            };
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
                    ? GetSameCountOfArrayItemsResult(source, target)
                    : GetDifferentCountOfArrayItemsResult(source, target)
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
                result.Add(GetIsInOneOfArraysResult(source, false, target, true, target[i].ToString(Formatting.None)));
            }

            return result;
        }

        private CompareResults CompareArrayItem(JArray source, JArray target, JToken sourceItemToCompare,
            List<int> foundTargetIndexes)
        {
            var result = new CompareResults();
            var settings = Settings.ArraySettings.FirstOrDefault(i => i.ArrayPath == $"$.{source.Path}");

            var IsInTarget = ArrayHelpers.FindInArray(target, sourceItemToCompare, foundTargetIndexes, settings);

            if (IsInTarget.Found) foundTargetIndexes.Add(IsInTarget.Index);

            if (IsInTarget.Found)
                HandleFoundInBothArrays(source, target, sourceItemToCompare, result, IsInTarget, settings);
            else
                result.Add(GetIsInOneOfArraysResult(source, true, target, false,
                    sourceItemToCompare.ToString(Formatting.None)));

            return result;
        }

        private void HandleFoundInBothArrays(JArray source, JArray target, JToken sourceItemToCompare,
            CompareResults result,
            (bool Found, JToken Item, int Index) IsInTarget, CompareArraySettings settings)
        {
            if (sourceItemToCompare.Type == JTokenType.Object)
                result.AddRange(CompareTokens(sourceItemToCompare, IsInTarget.Item));
            result.Add(GetInBothArraysResult(source, target, sourceItemToCompare.ToString(Formatting.None)));
            var sourceIndex = source.IndexOf(sourceItemToCompare);
            if (settings != null && settings.UniqueIndexMatching && IsInTarget.Index != sourceIndex)
                result.Add(GetInBothArraysDifferentIndexResult(source, sourceIndex, target, IsInTarget.Index,
                    sourceItemToCompare.ToString(Formatting.None)));
        }

        private CompareResult GetInBothArraysResult(JArray source, JArray target, string value)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetInBothArraysDifferentIndexResult(JArray source, int sourceIndex, JArray target,
            int targetIndex, string value)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetIsInOneOfArraysResult(JArray source, bool inSource, JArray target, bool inTarget,
            string value)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetDifferentCountOfArrayItemsResult(JArray source, JArray target)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetSameCountOfArrayItemsResult(JArray source, JArray target)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResults CompareIntegerNumbers(JToken source, JToken target)
        {
            var sourceValue = (long) source;
            var targetValue = (long) target;
            if (sourceValue == targetValue)
                return new CompareResults(GetSameValueResult(source, target,
                    sourceValue.ToString(CultureInfo.InvariantCulture)));

            if (sourceValue < targetValue)
                return new CompareResults(GetNumberValueDifferenceResult(source,
                    sourceValue.ToString(CultureInfo.InvariantCulture), target,
                    targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.LessThan));
            return new CompareResults(GetNumberValueDifferenceResult(source,
                sourceValue.ToString(CultureInfo.InvariantCulture), target,
                targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.greaterThan));
        }

        private CompareResults CompareFloatNumbers(JToken source, JToken target)
        {
            var sourceValue = (double) source;
            var targetValue = (double) target;
            if (Math.Abs(sourceValue - targetValue) < 0.0000000001)
                return new CompareResults(GetSameValueResult(source, target,
                    sourceValue.ToString(CultureInfo.InvariantCulture)));

            if (sourceValue < targetValue)
                return new CompareResults(GetNumberValueDifferenceResult(source,
                    sourceValue.ToString(CultureInfo.InvariantCulture), target,
                    targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.LessThan));
            return new CompareResults(GetNumberValueDifferenceResult(source,
                sourceValue.ToString(CultureInfo.InvariantCulture), target,
                targetValue.ToString(CultureInfo.InvariantCulture), eDifferenceSubType.greaterThan));
        }

        private CompareResult GetNumberValueDifferenceResult(JToken source, string sourceValue, JToken target,
            string targetValue, eDifferenceSubType subType)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResults CompareDeepEquals(JToken source, JToken target)
        {
            if (JToken.DeepEquals(source, target))
                return new CompareResults(GetSameValueResult(source, target,
                    source.ToString(Formatting.None)));

            return new CompareResults(GetDifferentValueResult(source, source.ToString(Formatting.None), target,
                target.ToString(Formatting.None)));
        }

        private CompareResults CompareBoolean(JToken source, JToken target)
        {
            var sourceValue = (bool) source;
            var targetValue = (bool) target;

            if (sourceValue == targetValue)
                return new CompareResults(GetSameValueResult(source, target, source.ToString(Formatting.None)));

            return new CompareResults(GetDifferentValueResult(source, sourceValue.ToString(), target,
                targetValue.ToString()));
        }

        private CompareResult GetSameValueResult(JToken source, JToken target, string value)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private static string GetPath(JToken token, IItemsFetcher fetcher)
        {
            return $"{fetcher.RootPathIndicator}{fetcher.PathDelimiter}{token.Path}";
        }

        private CompareResult GetDifferentValueResult(JToken source, string sourceValue, JToken target,
            string targetValue)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetDifferentTypeResult(JToken source, JToken target)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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

        private CompareResult GetSameTypeResult(JToken source, JToken target)
        {
            var sourcePath = GetPath(source, options.ItemsFetcher);
            var targetPath = GetPath(target, options.ItemsFetcher);
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
    }
}