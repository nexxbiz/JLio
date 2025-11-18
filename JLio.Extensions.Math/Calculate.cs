using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Text;

namespace JLio.Extensions.Math;

public class Calculate : FunctionBase
{
    private static readonly Regex TokenPattern = new(@"\{\{(.*?)\}\}", RegexOptions.Compiled);
    private static readonly Regex CommaPattern = new(@"(?<=\d),(?=\d)", RegexOptions.Compiled);
    private static readonly Regex[] ZeroPatterns = { new(@"/\s*0(?!\d)", RegexOptions.Compiled), new(@"/\s*0\.0+(?!\d)", RegexOptions.Compiled), new(@"/\s*\(\s*0\s*\)", RegexOptions.Compiled) };
    private static readonly ThreadLocal<DataTable> Table = new(() => new DataTable());
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    public Calculate() { }

    public Calculate(string expression)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(expression)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (!ValidateArguments(context)) return JLioFunctionResult.Failed(currentToken);
        var expression = GetExpression(currentToken, dataContext, context);
        if (expression == null) return JLioFunctionResult.Failed(currentToken);
        return ComputeResult(expression, context, currentToken);
    }

    private bool ValidateArguments(IExecutionContext context)
    {
        if (Arguments.Count == 1) return true;
        context.LogError(CoreConstants.FunctionExecution, $"failed: {FunctionName} requires 1 argument (expression)");
        return false;
    }

    private string? GetExpression(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var argument = GetArguments(Arguments, currentToken, dataContext, context).FirstOrDefault();
        if (argument?.Type != JTokenType.String) { LogError(context, "requires a string argument"); return null; }
        
        var stringValue = argument.Value<string>();
        if (stringValue == null) { LogError(context, "argument value is null"); return null; }
        
        try 
        { 
            return ProcessExpression(stringValue, currentToken, dataContext, context); 
        }
        catch (Exception ex) 
        { 
            LogError(context, $"token replacement failed: {ex.Message}"); 
            return null; 
        }
    }

    private string ProcessExpression(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var processedExpression = ReplaceTokens(expression, currentToken, dataContext, context).Trim('\'');
        if (string.IsNullOrWhiteSpace(processedExpression)) throw new Exception("expression is empty");
        processedExpression = CommaPattern.Replace(processedExpression, ".");
        if (HasDivisionByZero(processedExpression)) throw new Exception("division by zero detected");
        return processedExpression;
    }

    private JLioFunctionResult ComputeResult(string expression, IExecutionContext context, JToken currentToken)
    {
        try 
        { 
            var computeResult = Table.Value?.Compute(expression, null);
            if (computeResult == null || computeResult == DBNull.Value)
            {
                LogError(context, "computation returned null");
                return JLioFunctionResult.Failed(currentToken);
            }
            
            var result = Convert.ToDouble(computeResult); 
            return ValidateResult(result, context, currentToken); 
        }
        catch (EvaluateException) { LogError(context, "invalid mathematical expression"); return JLioFunctionResult.Failed(currentToken); }
        catch (SyntaxErrorException) { LogError(context, "syntax error in expression"); return JLioFunctionResult.Failed(currentToken); }
        catch (Exception ex) { LogError(context, $"computation failed: {ex.Message}"); return JLioFunctionResult.Failed(currentToken); }
    }

    private JLioFunctionResult ValidateResult(double value, IExecutionContext context, JToken currentToken)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) { LogError(context, "computation resulted in invalid number"); return JLioFunctionResult.Failed(currentToken); }
        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(value));
    }

    private string ReplaceTokens(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var matches = TokenPattern.Matches(expression);
        if (matches.Count == 0) return expression;
        var sb = new StringBuilder(expression);
        for (int i = matches.Count - 1; i >= 0; i--) ReplaceToken(sb, matches[i], currentToken, dataContext, context);
        return sb.ToString();
    }

    private void ReplaceToken(StringBuilder sb, Match match, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var inner = match.Groups[1].Value?.Trim();
        if (string.IsNullOrEmpty(inner))
        {
            throw new Exception("Empty token found");
        }
        
        var result = FixedValue.DefaultFunctionConverter.ParseString(inner).GetValue(currentToken, dataContext, context);
        if (!result.Success) throw new Exception($"Failed to resolve token: {inner}");
        
        // Check if path was not found (empty collection)
        if (result.Data.Count == 0) throw new Exception($"Token path not found: {inner}");
        
        // Check if we got exactly one result
        if (result.Data.Count != 1) throw new Exception($"Failed to resolve token: {inner}");
        
        var replacement = ConvertToNumeric(result.Data.First(), inner);
        sb.Remove(match.Index, match.Length).Insert(match.Index, replacement);
    }

    private string ConvertToNumeric(JToken token, string name)
    {
        return token.Type switch
        {
            JTokenType.Integer or JTokenType.Float => token.Value<double>().ToString(Invariant),
            JTokenType.String => ParseStringToNumeric(token.Value<string>() ?? string.Empty),
            JTokenType.Null => "0", // Treat null as 0 instead of throwing an error
            _ => throw new Exception($"Token type not supported: {token.Type}")
        };
    }

    private string ParseStringToNumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception("Token value is null or empty");
        }
        
        if (double.TryParse(value, NumberStyles.Float, Invariant, out var result)) return result.ToString(Invariant);
        if (double.TryParse(value, NumberStyles.Float, German, out result)) return result.ToString(Invariant);
        throw new Exception($"Token value is not numeric: {value}");
    }

    private bool HasDivisionByZero(string expression)
    {
        foreach (var pattern in ZeroPatterns) if (pattern.IsMatch(expression)) return true;
        return false;
    }

    private void LogError(IExecutionContext context, string message)
    {
        context.LogError(CoreConstants.FunctionExecution, $"{FunctionName} {message}");
    }
}