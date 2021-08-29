using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Merge : IJLioCommand
    {

        private IExecutionOptions executionOptions;
        private MergeSettings mergeSettings;
        public Merge()
        {

        }

        public Merge(string path, string targetPath)
        {
            Path = path;
            TargetPath = targetPath;
            mergeSettings = MergeSettings.CreateDefault();
        }

        public Merge(string path, string targetPath, MergeSettings settings)
        {
            Path = path;
            TargetPath = targetPath;
            mergeSettings = settings;
        }

        public string CommandName => "merge";

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("targetPath")]
        public string TargetPath { get; set; }



        public JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options)
        {
            executionOptions = options;

            var sourceTokens = options.ItemsFetcher.SelectTokens(Path, dataContext);
            var targetTokens = options.ItemsFetcher.SelectTokens(TargetPath, dataContext);

            sourceTokens.ForEach(s =>
            targetTokens.ForEach(t => MergeElements(s, t)));
            return new JLioExecutionResult(true, dataContext);
        }

        public ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult { IsValid = true };
            if (string.IsNullOrWhiteSpace(Path))
            {
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");
                result.IsValid = false;
            }
            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                result.ValidationMessages.Add($"Target Path property for {CommandName} command is missing");
                result.IsValid = false;
            }

            return result;
        }
        
        private void MergeElements(JToken source, JToken target)
        {

            if (AreDifferentTypes(source, target))
            {
                HandleDifferentTypes(source, target);
            }
            else if (IsComplexType(source))
            {
                HandleComplexMerge(source, target);
            }
            else if (IsArrayType(source))
            {
                HandleArrayMerge((JArray)source, (JArray)target);
            }
        }

        private void HandleArrayMerge(JArray source, JArray target)
        {

            var arraySettings = GetMergeSettingsForArray(target);
            foreach (var child in source.Children())
            {
                AddToArray(target, child, arraySettings);
            }

        }

        private void AddToArray(JArray target, JToken itemToAdd, MergeArraySettings arraySettings)
        {

            if (arraySettings.KeyPaths.Any())
            {
                AdditemsWithKeyMatch(target, itemToAdd, arraySettings);
            }
            else if (!arraySettings.UniqueItemsWithoutKeys || !IsItemInTarget(target, itemToAdd))
            {
                target.Add(itemToAdd);
            }
        }

        private void AdditemsWithKeyMatch(JArray target, JToken itemToAdd, MergeArraySettings arraySettings)
        {
            var targetItems = FindTargetArrayElementForKeys(target, itemToAdd, arraySettings.KeyPaths);
            if (!targetItems.Any())
                target.Add(itemToAdd);
            else
                targetItems.ForEach(t => MergeElements(itemToAdd, t));
        }

        internal List<JToken> FindTargetArrayElementForKeys(JToken target, JToken itemToMatch, List<string> keys)
        {
            var result = new List<JToken>();


            if (keys.Any())
            {
                target.Where(t =>
                             {
                                return AllKeyMatch(t, keys, itemToMatch);
                            }
                    ).ToList()
                    .ForEach(t => result.Add(t));
            }
            else
            {
                target.Where(t => JToken.DeepEquals(t, itemToMatch)).ToList()
                    .ForEach(t => result.Add(t));
            }

            return result;
        }

        internal bool AllKeyMatch(JToken targetItem, List<string> keys, JToken itemToMatch)
        {

            var matchResult = keys.All(k =>
            {
                var result = AreTheSameValue(k, targetItem, itemToMatch);

                return result;
            });
            return matchResult;
        }

        private bool AreTheSameValue(string jsonPath, JToken firstElement, JToken secondElement)
        {

            jsonPath = $"{JLioConstants.RootPathIndicator}.{jsonPath}"; //TODO: TRICKY , WATCH CLOSELY
            var firstKeyValue = firstElement.SelectToken(jsonPath);
            var secondKeyValue = secondElement.SelectToken(jsonPath);

            return JToken.DeepEquals(firstKeyValue, secondKeyValue);
        }

        private bool IsItemInTarget(JArray target, JToken itemToAdd)
        {
            return target.Any(t => JToken.DeepEquals(t, itemToAdd));
        }

        private MergeArraySettings GetMergeSettingsForArray(JToken jtoken)
        {
            return mergeSettings
            .ArraySettings
            .FirstOrDefault(a => a.ArrayPath == GetPathFor(jtoken))
            ?? new MergeArraySettings();
        }

        private string GetPathFor(JToken jtoken)
        {
            if (jtoken == null)
                return JLioConstants.RootPathIndicator;
            if (jtoken.Type == JTokenType.Array || jtoken.Type == JTokenType.Object)
                return GetPathFor(jtoken.Parent);
            if (jtoken.Type == JTokenType.Property)
                return $"{GetPathFor(jtoken.Parent)}.{((JProperty)jtoken).Name}";

            return string.Empty;
        }

        private bool IsArrayType(JToken source)
        {
            return source.Type == JTokenType.Array;
        }

        private bool IsComplexType(JToken source)
        {
            return source.Type == JTokenType.Object;
        }

        private void HandleComplexMerge(JToken source, JToken target)
        {
            var MatchSuccess = ObjectsMatchOnMatchSettings(source, target);

            if (MatchSuccess || !mergeSettings.MatchSettings.HasKeys)
                foreach (var property in ((JObject)source).Properties())
                    MergeProperties(property, (JObject)target);
        }

        private bool ObjectsMatchOnMatchSettings(JToken source, JToken target)
        {
            var result = AllKeyMatch(target, mergeSettings.MatchSettings.KeyPaths, source);
            return mergeSettings.MatchSettings.HasKeys && result;
        }
        
        private void MergeProperties(JProperty property, JObject target)
        {
            if (target.Properties().Any(p => p.Name == property.Name))
            {
                SetValueForProperty(target, property);
            }
            else
            {
                if (mergeSettings.Strategy != MergeSettings.STRATEGY_ONLY_VALUES) AddProperty(target, property);
            }
        }

        private void SetValueForProperty(JObject target, JProperty property)
        {
            if (AreDifferentTypes(target[property.Name], property.Value) || !IsComplexType(property.Value))
            {
                if (mergeSettings.Strategy != MergeSettings.STRATEGY_ONLY_STRUCTURE)
                    target[property.Name].Replace(property.Value);
            }
            else
            {
                HandleComplexMerge(property.Value, target[property.Name]);
            }
        }

        private void AddProperty(JObject target, JProperty property)
        {
            target.Add(property.Name, property.Value);
        }

        private void HandleDifferentTypes(JToken source, JToken target)
        {
            target.Replace(source);
        }

        private bool AreDifferentTypes(JToken source, JToken target)
        {
            return source.Type != target.Type;
        }

    }

    public class MergeSettings
    {
        public const string STRATEGY_FULLMERGE = "fullMerge";
        public const string STRATEGY_ONLY_STRUCTURE = "onlyStructure";
        public const string STRATEGY_ONLY_VALUES = "onlyValues";

        [JsonProperty("strategy")] public string Strategy { get; set; } = STRATEGY_FULLMERGE;

        [JsonProperty("arraySettings")]
        public List<MergeArraySettings> ArraySettings { get; set; } = new List<MergeArraySettings>();

        [JsonProperty("matchSettings")]
        public MatchSettings MatchSettings { get; set; } = new MatchSettings();

        public static MergeSettings CreateDefault()
        {
            return new MergeSettings
            {

            };
        }
    }

    public class MergeArraySettings
    {
        [JsonProperty("arrayPath")]
        public string ArrayPath { get; set; } = string.Empty;

        [JsonProperty("keyPaths")]
        public List<string> KeyPaths { get; set; } = new List<string>();

        [JsonProperty("uniqueItemsWithoutKeys")]
        public bool UniqueItemsWithoutKeys { get; set; } = false;
    }

    public class MatchSettings
    {
        [JsonProperty("keyPaths")]
        public List<string> KeyPaths { get; set; } = new List<string>();
        public bool HasKeys => KeyPaths.Any();
    }
}
