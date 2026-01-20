using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.TimeDate;

/// <summary>
/// Compare two dates and return -1, 0, or 1
/// </summary>
public class DateCompare : FunctionBase
{
    public DateCompare()
    {
    }

    public DateCompare(params string[] arguments)
    {
        foreach (var a in arguments)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));
        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (!ValidateArguments(context))
            return JLioFunctionResult.Failed(currentToken);

        var arguments = GetArguments(Arguments, currentToken, dataContext, context);

        if (!TryParseDate(arguments[0], out var date1))
        {
            context.LogError(CoreConstants.FunctionExecution, 
                $"{FunctionName} cannot parse first date: {arguments[0]}");
            return JLioFunctionResult.Failed(currentToken);
        }

        if (!TryParseDate(arguments[1], out var date2))
        {
            context.LogError(CoreConstants.FunctionExecution, 
                $"{FunctionName} cannot parse second date: {arguments[1]}");
            return JLioFunctionResult.Failed(currentToken);
        }

        var comparison = DateTime.Compare(date1, date2);
        return new JLioFunctionResult(true, new JValue(comparison));
    }

    private bool ValidateArguments(IExecutionContext context)
    {
        if (Arguments.Count == 2) 
            return true;
        
        context.LogError(CoreConstants.FunctionExecution, 
            $"{FunctionName} requires exactly two arguments: date1, date2");
        return false;
    }

    private static bool TryParseDate(JToken token, out DateTime date)
    {
        date = default;
        
        switch (token.Type)
        {
            case JTokenType.Date:
                date = token.Value<DateTime>();
                return true;
            
            case JTokenType.String:
                var stringValue = token.Value<string>();
                if (string.IsNullOrWhiteSpace(stringValue))
                    return false;

                // Remove surrounding quotes if they exist
                stringValue = stringValue.Trim('\'', '"');

                // Try parsing as UTC first if it ends with Z
                if (stringValue.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, 
                        DateTimeStyles.RoundtripKind, out date))
                    {
                        date = date.ToUniversalTime();
                        return true;
                    }
                }

                // Try various date formats in order of specificity (most specific first)
                var formats = new[]
                {
                    // ISO 8601 formats (culture-independent)
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ss.ffZ", 
                    "yyyy-MM-ddTHH:mm:ss.fZ",
                    "yyyy-MM-ddTHH:mm:ssZ", 
                    "yyyy-MM-ddTHH:mm:ss.fff",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd",
                    
                    // Only unambiguous regional formats
                    "yyyy/MM/dd",
                    "yyyy-MM-dd",
                    "dd-MMM-yyyy",  // e.g., "15-Jan-2024"
                    "MMM dd, yyyy", // e.g., "Jan 15, 2024"
                    
                    // Avoid ambiguous MM/dd vs dd/MM formats
                };

                // First try with InvariantCulture and specific formats
                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(stringValue, format, CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, out date))
                        return true;
                }
                
                // Only as last resort, try general parsing with InvariantCulture
                if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out date))
                    return true;
                    
                return false;
                
            case JTokenType.Integer:
                // Assume Unix timestamp
                try
                {
                    var timestamp = token.Value<long>();
                    date = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                    return true;
                }
                catch
                {
                    return false;
                }
                
            default:
                return false;
        }
    }
}