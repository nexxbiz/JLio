using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Partial : FunctionBase
    {
        public Partial()
        {
        }

        public Partial(params string[] arguments)
        {
            arguments.ToList().ForEach(a =>
                this.arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{a}\"")))));
        }

        public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            if (!Validate(currentToken, context)) return new JLioFunctionResult(false, currentToken);

            var currentPath = currentToken.Path;
            var values = GetArguments(arguments, currentToken, dataContext, context);
            var pathsToRemove = GetPathsToRemove(currentToken, values, context);
            var result = currentToken.DeepClone();
            pathsToRemove.ToList().ForEach(i => Remove(i, result));
            return new JLioFunctionResult(true, result);
        }

        private void Remove(string path, JToken result)
        {
            var tokens = result.SelectTokens(path);
            tokens.ToList().ForEach(t => JsonMethods.RemoveItemFromTarget(t));
        }

        private IEnumerable<string> GetPathsToRemove(JToken currentToken, List<JToken> values,
            IExecutionContext context)
        {
            var sourcePaths = JsonMethods.GetAllElements(currentToken).Select(i => i.Path).ToList();
            var selectionPaths = new List<JToken>();
            values.ForEach(v => selectionPaths.AddRange(JsonMethods.GetAllElements(v)));
            selectionPaths.Select(s => s.Path).ToList().ForEach(p =>
            {
                RemoveSelectionItems(p, sourcePaths);
                RemoveLeadingElementsOfSelectionItems(p,
                    sourcePaths); //remove all items that are in the path of the selections
            });
            sourcePaths = OptimizeRemovePaths(sourcePaths);
            sourcePaths = RemoveLeadingPath(sourcePaths, currentToken.Path, context);
            return sourcePaths.Distinct();
        }

        private List<string> RemoveLeadingPath(List<string> sourcePaths, string path, IExecutionContext context)
        {
            var prefixPath = $"{context.ItemsFetcher.RootPathIndicator}{context.ItemsFetcher.PathDelimiter}";
            return sourcePaths.Select(i => $"{prefixPath}{i.Substring(path.Length)}").ToList();
        }

        private static List<string> OptimizeRemovePaths(List<string> sourcePaths)
        {
            var helperList = new List<string>();
            helperList.AddRange(sourcePaths.Distinct());
            helperList.ForEach(i => { sourcePaths.RemoveAll(s => s.StartsWith(i) && s.Length > i.Length); });
            return helperList;
        }

        private static void RemoveLeadingElementsOfSelectionItems(string path, List<string> sourcePaths)
        {
            sourcePaths.RemoveAll(path.StartsWith);
        }

        private static void RemoveSelectionItems(string path, List<string> sourcePaths)
        {
            sourcePaths.RemoveAll(x => x.StartsWith(path));
        }

        private bool Validate(JToken currentToken, IExecutionContext context)
        {
            if (arguments.Count < 1)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"failed: {FunctionName} requires at least 1 argument");
                {
                    return false;
                }
            }

            return true;
        }
    }
}