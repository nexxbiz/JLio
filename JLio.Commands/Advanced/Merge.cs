using System.Linq;
using JLio.Commands.Advanced.Settings;
using JLio.Commands.Logic;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Advanced
{
    public class Merge : IJLioCommand
    {
        private IExecutionOptions executionOptions;

        public Merge()
        {
        }

        public Merge(string path, string targetPath)
        {
            Path = path;
            TargetPath = targetPath;
            Settings = MergeSettings.CreateDefault();
        }

        public Merge(string path, string targetPath, MergeSettings settings)
        {
            Path = path;
            TargetPath = targetPath;
            Settings = settings;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("settings")]
        public MergeSettings Settings { get; set; }

        [JsonProperty("targetPath")]
        public string TargetPath { get; set; }

        [JsonProperty("command")]
        public string CommandName => "merge";

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
            var result = new ValidationResult {IsValid = true};
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
                HandleDifferentTypes(source, target);
            else if (IsComplexType(source))
                HandleComplexMerge(source, target);
            else if (IsArrayType(source)) HandleArrayMerge((JArray) source, (JArray) target);
        }

        private void HandleArrayMerge(JArray source, JArray target)
        {
            var arraySettings = GetMergeSettingsForArray(target);
            foreach (var child in source.Children()) AddToArray(target, child, arraySettings);
        }

        private void AddToArray(JArray target, JToken itemToAdd, MergeArraySettings arraySettings)
        {
            if (arraySettings.KeyPaths.Any())
                AdditemsWithKeyMatch(target, itemToAdd, arraySettings);
            else if (!arraySettings.UniqueItemsWithoutKeys || !ArrayHelpers.IsItemInArray(target, itemToAdd))
                target.Add(itemToAdd);
        }

        private void AdditemsWithKeyMatch(JArray target, JToken itemToAdd, MergeArraySettings arraySettings)
        {
            var targetItems = ArrayHelpers.FindTargetArrayElementForKeys(target, itemToAdd, arraySettings.KeyPaths);
            if (!targetItems.Any())
                target.Add(itemToAdd);
            else
                targetItems.ForEach(t => MergeElements(itemToAdd, t));
        }

        private MergeArraySettings GetMergeSettingsForArray(JToken jtoken)
        {
            return Settings
                       .ArraySettings
                       .FirstOrDefault(a => a.ArrayPath == GetPathFor(jtoken))
                   ?? new MergeArraySettings();
        }

        private string GetPathFor(JToken jtoken)
        {
            if (jtoken == null)
                return executionOptions.ItemsFetcher.RootPathIndicator;
            if (jtoken.Type == JTokenType.Array || jtoken.Type == JTokenType.Object)
                return GetPathFor(jtoken.Parent);
            if (jtoken.Type == JTokenType.Property)
                return $"{GetPathFor(jtoken.Parent)}.{((JProperty) jtoken).Name}";

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

            if (MatchSuccess || !Settings.MatchSettings.HasKeys)
                foreach (var property in ((JObject) source).Properties())
                    MergeProperties(property, (JObject) target);
        }

        private bool ObjectsMatchOnMatchSettings(JToken source, JToken target)
        {
            var result = ArrayHelpers.AllKeyMatch(target, Settings.MatchSettings.KeyPaths, source);
            return Settings.MatchSettings.HasKeys && result;
        }

        private void MergeProperties(JProperty property, JObject target)
        {
            if (target.Properties().Any(p => p.Name == property.Name))
            {
                SetValueForProperty(target, property);
            }
            else
            {
                if (Settings.Strategy != MergeSettings.STRATEGY_ONLY_VALUES) AddProperty(target, property);
            }
        }

        private void SetValueForProperty(JObject target, JProperty property)
        {
            if (AreDifferentTypes(target[property.Name], property.Value) || !IsComplexType(property.Value))
            {
                if (Settings.Strategy != MergeSettings.STRATEGY_ONLY_STRUCTURE)
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
}