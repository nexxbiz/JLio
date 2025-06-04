using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class Partial : FunctionBase
{
    public Partial()
    {
    }

    public Partial(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a, null))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (!Validate(currentToken, context)) return new JLioFunctionResult(false, currentToken);
        var firstArgument = Arguments.First().GetStringRepresentation();
        if (firstArgument.StartsWith(context.ItemsFetcher.RootPathIndicator))
        {
            var source = context.ItemsFetcher.SelectTokens(firstArgument, dataContext);
            if (source.Count != 1)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"The first argument of function: {FunctionName} should return 1 item when a {context.ItemsFetcher.RootPathIndicator} is used. Now {source.Count} tokens were selected.");
                return JLioFunctionResult.Failed(currentToken);
            }

            Arguments.RemoveAt(0);
            var values = GetArguments(Arguments, source.First(), dataContext, context);
            return ExecutePartial(source.First(), dataContext, context, values);
        }
        else
        {
            var values = GetArguments(Arguments, currentToken, dataContext, context);
            return ExecutePartial(currentToken, dataContext, context, values);
        }
    }

    private JLioFunctionResult ExecutePartial(JToken source, JToken dataContext, IExecutionContext context,
        List<JToken> values)
    {
        var pathsToRemove = GetPathsToRemove(source, values, context);
        var result = source.DeepClone();
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
        var sourcePaths = currentToken.GetAllElements().Select(i => i.Path).ToList();
        var selectionPaths = new List<JToken>();
        values.Where(v  => !JToken.DeepEquals(v, JValue.CreateNull())).ToList().ForEach(v => selectionPaths.AddRange(v.GetAllElements()));
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
        return sourcePaths.Select(i => $"{prefixPath}{i.Substring(path.Length+1)}").ToList();
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
        if (Arguments.Count < 1)
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