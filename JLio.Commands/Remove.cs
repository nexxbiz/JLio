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
    public class Remove : IJLioCommand
    {
        private IExecutionOptions executionOptions;

        public Remove()
        {
        }

        public Remove(string path)
        {
            Path = path;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        public string CommandName => "remove";

        public JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options)
        {
            executionOptions = options;
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i => options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            };
            RemoveItems(dataContext);

            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"{CommandName}: completed for {Path}");

            return new JLioExecutionResult(true, dataContext);
        }

        private void RemoveItems(JToken data)
        {
            var targetItems =
                executionOptions.ItemsFetcher.SelectTokens(Path, data);
            if (targetItems.Count == 0)
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"{Path} did not retrieve any items");
            targetItems.ForEach(RemoveItemFromTarget);
        }

        private void RemoveItemFromTarget(JToken selectedValue)
        {
            var parent = selectedValue.Parent;
            switch (parent?.Type)
            {
                case JTokenType.Property:
                    parent.Remove();
                    break;
                case JTokenType.Array:
                    RemoveValuesFromArray((JArray)parent, selectedValue);
                    break;
                default:
                    executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                        $"{CommandName} only works on properties or items in array's");
                    break;
            }
        }

        private void RemoveValuesFromArray(JArray array, JToken selectedValue)
        {
            var index = array.IndexOf(selectedValue);
            array.RemoveAt(index);
        }

        public ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult() { IsValid = true };
            if (string.IsNullOrWhiteSpace(Path))
            {
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");
                result.IsValid = false;
            }
            return result;
        }
    }
}
