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
        private IJLioExecutionOptions executionOptions;

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

        public JLioExecutionResult Execute(JToken dataContext, IJLioExecutionOptions options)
        {
            executionOptions = options;
            if (!ValidateCommandInstance()) return new JLioExecutionResult(false,  dataContext);
            var targetPath = JsonPathMethods.SplitPath(Path);
            RemoveItems(dataContext);
        
            
            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"{CommandName}: completed for {targetPath.Elements.ToPathString()}");
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


        public bool ValidateCommandInstance()
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(Path))
            {
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Path property for {CommandName} command is missing");
                result = false;
            }

            if (result == false)
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, "Command not executed");
            return result;
        }
    }
}
