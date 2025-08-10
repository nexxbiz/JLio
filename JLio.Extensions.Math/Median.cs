using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Median : FunctionBase
{
    public Median()
    {
    }

    public Median(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count == 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires at least one argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        var numbers = new List<double>();

        foreach (var token in values)
        {
            if (!TryCollectNumbers(token, numbers, context))
            {
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        if (numbers.Count == 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} found no valid numeric values");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Sort the numbers to find median
        numbers.Sort();

        double median;
        int count = numbers.Count;
        
        if (count % 2 == 0)
        {
            // Even number of elements - average of middle two
            median = (numbers[count / 2 - 1] + numbers[count / 2]) / 2.0;
        }
        else
        {
            // Odd number of elements - middle element
            median = numbers[count / 2];
        }

        return new JLioFunctionResult(true, new JValue(median));
    }

    private bool TryCollectNumbers(JToken token, List<double> numbers, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                numbers.Add(token.Value<double>());
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                numbers.Add(numeric);
                return true;

            case JTokenType.Null:
                return true;

            case JTokenType.Array:
                return TryCollectNumbersFromArray((JArray)token, numbers, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryCollectNumbersFromArray(JArray array, List<double> numbers, IExecutionContext context)
    {
        foreach (var item in array)
        {
            if (!TryCollectNumbers(item, numbers, context))
            {
                return false;
            }
        }
        return true;
    }
}