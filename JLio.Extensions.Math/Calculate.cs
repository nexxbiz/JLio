using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Math;

public class Calculate : FunctionBase
{
    public Calculate()
    {
    }

    public Calculate(string expression)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(expression)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Count != 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires 1 argument (expression)");
            return JLioFunctionResult.Failed(currentToken);
        }

        var argument = GetArguments(Arguments, currentToken, dataContext, context).FirstOrDefault();
        if (argument == null || argument.Type != JTokenType.String)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires a string argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        var expression = argument.Value<string>();

        try
        {
            expression = ReplaceTokens(expression, currentToken, dataContext, context);
        }
        catch
        {
            context.LogError(CoreConstants.FunctionExecution, $"{FunctionName} transformation failed");
            return JLioFunctionResult.Failed(currentToken);
        }

        try
        {
            var table = new DataTable();
            var computeResult = table.Compute(expression, null);
            var value = Convert.ToDouble(computeResult);
            return new JLioFunctionResult(true, new JValue(value));
        }
        catch
        {
            context.LogError(CoreConstants.FunctionExecution, $"{FunctionName} transformation failed");
            return JLioFunctionResult.Failed(currentToken);
        }
    }

    private string ReplaceTokens(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var pattern = @"\[(.*?)\]";
        var matches = Regex.Matches(expression, pattern);
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var inner = match.Groups[1].Value.Trim();
            var valueProvider = FixedValue.DefaultFunctionConverter.ParseString(inner);
            var result = valueProvider.GetValue(currentToken, dataContext, context);
            if (!result.Success || result.Data.Count != 1)
                throw new Exception("invalid token count");

            var token = result.Data.First();
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                expression = expression.Remove(match.Index, match.Length)
                    .Insert(match.Index, token.Value<double>().ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (token.Type == JTokenType.String &&
                     double.TryParse(token.Value<string>(), out var numeric))
            {
                expression = expression.Remove(match.Index, match.Length)
                    .Insert(match.Index, numeric.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                throw new Exception("non numeric");
            }
        }

        return expression;
    }
}

