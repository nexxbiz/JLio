using System.Collections.Generic;
using JLio.Commands.Advanced.Settings;
using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Advanced;

public class Distinct : CommandBase
{
    private IExecutionContext executionContext;

    public Distinct()
    {
    }

    public Distinct(string path)
    {
        Path = path;
    }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("keyPaths")]
    public List<string> KeyPaths { get; set; } = new List<string>();

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        executionContext = context;
        var validationResult = ValidateCommandInstance();
        if (!validationResult.IsValid)
        {
            validationResult.ValidationMessages.ForEach(i =>
                context.LogWarning(CoreConstants.CommandExecution, i));
            return new JLioExecutionResult(false, dataContext);
        }

        var targets = context.ItemsFetcher.SelectTokens(Path, dataContext);
        foreach (var target in targets)
        {
            if (target is not JArray array)
            {
                context.LogWarning(CoreConstants.CommandExecution,
                    $"{CommandName}: {target.Path} is not an array");
                continue;
            }

            var arraySettings = new MergeArraySettings
            {
                KeyPaths = KeyPaths,
                UniqueItemsWithoutKeys = true
            };

            var resultArray = new JArray();
            foreach (var child in array.Children())
                AddToArray(resultArray, child.DeepClone(), arraySettings);

            array.Replace(resultArray);
        }

        return new JLioExecutionResult(true, dataContext);
    }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Path))
            result.ValidationMessages.Add($"Path property for {CommandName} command is missing");
        return result;
    }

    private void AddToArray(JArray target, JToken itemToAdd, MergeArraySettings arraySettings)
    {
        if (arraySettings.KeyPaths.Count > 0)
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
            targetItems.ForEach(t => MergeTokens(itemToAdd, t, arraySettings));
    }

    private void MergeTokens(JToken source, JToken target, MergeArraySettings arraySettings)
    {
        var tmp = new JObject
        {
            ["source"] = source,
            ["target"] = target
        };

        var settings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
            {
                new MergeArraySettings
                {
                    ArrayPath = "$.target",
                    KeyPaths = arraySettings.KeyPaths,
                    UniqueItemsWithoutKeys = arraySettings.UniqueItemsWithoutKeys
                }
            }
        };

        var merge = new Merge("$.source", "$.target", settings);
        merge.Execute(tmp, executionContext);
    }
}

