using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Text;

public class ToLower : FunctionBase
{
    public ToLower()
    {
    }

    public ToLower(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 1 || values.Count > 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 1 or 2 arguments (text, [culture])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        
        string result;
        if (values.Count == 2)
        {
            var cultureName = TextHelpers.GetStringValue(values[1]);
            try
            {
                var culture = string.IsNullOrEmpty(cultureName) || cultureName.Equals("invariant", StringComparison.OrdinalIgnoreCase)
                    ? CultureInfo.InvariantCulture
                    : CultureInfo.GetCultureInfo(cultureName);
                result = text.ToLower(culture);
            }
            catch (CultureNotFoundException)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} invalid culture name: {cultureName}");
                return JLioFunctionResult.Failed(currentToken);
            }
        }
        else
        {
            result = text.ToLowerInvariant();
        }

        return new JLioFunctionResult(true, new JValue(result));
    }
}