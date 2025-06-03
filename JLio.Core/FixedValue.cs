using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core;

public class FixedValue : IFunction
{
    public FixedValue(JToken value)
    {
        Value = value;
    }

    public JToken Value { get; }

    public string FunctionName => "FixedValue";

    public IFunction SetArguments(Arguments newArguments)
    {
        return this;
    }

    public JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Value.Type == JTokenType.String) return HandleString(currentToken, dataContext, context);
        return new JLioFunctionResult(true, Value);
    }

    public string ToScript()
    {
        return Value.ToString();
    }

    private JLioFunctionResult HandleString(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var stringValue = Value.ToString();

        if (stringValue.StartsWith(context.ItemsFetcher.CurrentItemPathIndicator,
            StringComparison.InvariantCulture))
            return new JLioFunctionResult(true,
                context.ItemsFetcher.SelectTokens(
                    $"{context.ItemsFetcher.RootPathIndicator}{stringValue.Substring(context.ItemsFetcher.CurrentItemPathIndicator.Length)}"
                    , currentToken));
        if (stringValue.StartsWith(context.ItemsFetcher.RootPathIndicator, StringComparison.InvariantCulture))
            return new JLioFunctionResult(true, context.ItemsFetcher.SelectTokens(stringValue, dataContext));
        return new JLioFunctionResult(true, Value);
    }
}