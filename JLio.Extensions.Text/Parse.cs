﻿using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Parse : FunctionBase
{
    public Parse()
    {
    }

    public Parse(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Any()) return ExecuteWithArguments(currentToken, dataContext, context);
        return ExecuteWithoutArguments(currentToken, context);
    }

    private JLioFunctionResult ExecuteWithArguments(JToken currentToken, JToken dataContext,
        IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context).Where(i => i != null)
            .Select(i => i.ToString()).ToList();
        return !values.Any()
            ? new JLioFunctionResult(false, JValue.CreateNull())
            : TryParse(values.First(), context);
    }

    private JLioFunctionResult ExecuteWithoutArguments(JToken currentToken, IExecutionContext context)
    {
        if (currentToken.Type == JTokenType.String)
            return TryParse(currentToken.Value<string>() ?? currentToken.ToString(), context);

        context.LogWarning(CoreConstants.FunctionExecution,
            $"Function {FunctionName} only works on type string. Current type = {currentToken.Type}!");
        return new JLioFunctionResult(false, currentToken);
    }

    private JLioFunctionResult TryParse(string value, IExecutionContext context)
    {
        try
        {
            return new JLioFunctionResult(true, JToken.Parse(value));
        }
        catch (Exception)
        {
            context.Logger.Log(LogLevel.Warning, CoreConstants.FunctionExecution,
                $"Function {FunctionName} failed: unable to parse {value}");

            return new JLioFunctionResult(false, value);
        }
    }
}