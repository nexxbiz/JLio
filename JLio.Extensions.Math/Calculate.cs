using System.Data;
using System.Globalization;
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
            expression = ReplaceTokens(expression, currentToken, dataContext, context).Trim('\'');
        }
        catch (Exception ex)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} token replacement failed: {ex.Message}");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Handle European decimal notation (comma as decimal separator)
        expression = NormalizeDecimalNotation(expression);

        try
        {
            // Validate expression before computation
            if (string.IsNullOrWhiteSpace(expression))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} expression is empty");
                return JLioFunctionResult.Failed(currentToken);
            }

            // Check for division by zero before computation
            if (ContainsDivisionByZero(expression))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} division by zero detected");
                return JLioFunctionResult.Failed(currentToken);
            }

            var table = new DataTable();
            var computeResult = table.Compute(expression, null);

            if (computeResult == null || computeResult == DBNull.Value)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} computation returned null result");
                return JLioFunctionResult.Failed(currentToken);
            }

            var value = Convert.ToDouble(computeResult);

            // Check for invalid results (NaN, Infinity)
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} computation resulted in invalid number");
                return JLioFunctionResult.Failed(currentToken);
            }

            return new JLioFunctionResult(true, new JValue(value));
        }
        catch (EvaluateException)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} invalid mathematical expression");
            return JLioFunctionResult.Failed(currentToken);
        }
        catch (SyntaxErrorException)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} syntax error in expression");
            return JLioFunctionResult.Failed(currentToken);
        }
        catch (Exception ex)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} computation failed: {ex.Message}");
            return JLioFunctionResult.Failed(currentToken);
        }
    }

    private string ReplaceTokens(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var pattern = @"\{\{(.*?)\}\}";
        var matches = Regex.Matches(expression, pattern);

        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var inner = match.Groups[1].Value.Trim();
            var valueProvider = FixedValue.DefaultFunctionConverter.ParseString(inner);
            var result = valueProvider.GetValue(currentToken, dataContext, context);

            if (!result.Success)
            {
                throw new Exception($"Failed to resolve token: {inner}");
            }

            if (result.Data.Count != 1)
            {
                throw new Exception($"Token resolution returned multiple values for: {inner}. Expected single value.");
            }

            var token = result.Data.First();

            // Handle different token types
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                var numericValue = token.Value<double>();
                expression = expression.Remove(match.Index, match.Length)
                    .Insert(match.Index, numericValue.ToString(CultureInfo.InvariantCulture));
            }
            else if (token.Type == JTokenType.String)
            {
                var stringValue = token.Value<string>();
                if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericFromString))
                {
                    expression = expression.Remove(match.Index, match.Length)
                        .Insert(match.Index, numericFromString.ToString(CultureInfo.InvariantCulture));
                }
                else if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.GetCultureInfo("de-DE"), out var numericFromStringComma))
                {
                    // Handle European decimal notation in string values
                    expression = expression.Remove(match.Index, match.Length)
                        .Insert(match.Index, numericFromStringComma.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new Exception($"Token value is not numeric: {stringValue}");
                }
            }
            else if (token.Type == JTokenType.Null)
            {
                throw new Exception($"Token resolved to null: {inner}");
            }
            else
            {
                throw new Exception($"Token type not supported for calculation: {token.Type}");
            }
        }

        return expression;
    }

    private string NormalizeDecimalNotation(string expression)
    {
        // Replace European decimal notation (comma as decimal separator) with period
        // This regex matches numbers with comma as decimal separator
        // Positive lookbehind and lookahead to ensure we're dealing with decimal numbers
        var commaDecimalPattern = @"(?<=\d),(?=\d)";
        return Regex.Replace(expression, commaDecimalPattern, ".");
    }

    private bool ContainsDivisionByZero(string expression)
    {
        // Simple check for obvious division by zero cases
        // This won't catch all cases (like variables that evaluate to zero), 
        // but will catch literal zero divisions
        var divisionByZeroPatterns = new[]
        {
            @"/\s*0(?!\d)",  // "/0" not followed by another digit
            @"/\s*0\.0+(?!\d)",  // "/0.0" or "/0.00" etc.
            @"/\s*\(\s*0\s*\)"  // "/(0)"
        };

        return divisionByZeroPatterns.Any(pattern => Regex.IsMatch(expression, pattern));
    }
}