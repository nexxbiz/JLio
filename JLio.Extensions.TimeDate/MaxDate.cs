using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.TimeDate;

/// <summary>
/// Find the maximum (latest) date from multiple date values
/// </summary>
public class MaxDate : FunctionBase
{
    public MaxDate()
    {
    }

    public MaxDate(params string[] arguments)
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
        var dates = new List<DateTime>();

        foreach (var arg in arguments)
        {
            if (arg.Type == JTokenType.Array)
            {
                // Handle array of dates
                foreach (var arrayItem in arg)
                {
                    if (TryParseDate(arrayItem, out var arrayDate))
                        dates.Add(arrayDate);
                }
            }
            else if (TryParseDate(arg, out var date))
            {
                dates.Add(date);
            }
            else
            {
                context.LogError(CoreConstants.FunctionExecution, 
                    $"{FunctionName} cannot parse date value: {arg}");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        if (dates.Count == 0)
        {
            context.LogError(CoreConstants.FunctionExecution, 
                $"{FunctionName} requires at least one valid date");
            return JLioFunctionResult.Failed(currentToken);
        }

        var maxDate = dates.Max();
        return new JLioFunctionResult(true, new JValue(maxDate.ToString("O")));
    }

    private bool ValidateArguments(IExecutionContext context)
    {
        if (Arguments.Count >= 1) 
            return true;
        
        context.LogError(CoreConstants.FunctionExecution, 
            $"{FunctionName} requires at least one argument");
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