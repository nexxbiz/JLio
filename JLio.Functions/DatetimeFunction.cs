using System;
using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    //> - datetime()
    //> - datetime(UTC)
    //> - datetime(startOfDay)
    //> - datetime(startofDayUTC)
    //> - datetime('dd-MM-yyyy HH:mm:ss')
    //> - datetime(UTC, 'dd-MM-yyyy HH:mm:ss')
    //> - datetime(startOfDay,'dd-MM-yyyy HH:mm:ss'
    //> - datetime(startOfDayUTC, 'dd-MM-yyyy HH:mm:ss')

    // default:
    // timeselection , local time now
    // format : 2012-04-23T18:25:43.511Z
    public class DatetimeFunction : FunctionBase
    {


        public DatetimeFunction() : base("datetime")
        {
        }

        public DatetimeFunction(params string[] arguments) : base("datetime")
        {
            arguments.ToList().ForEach(a =>
                this.arguments.Add(new JLioFunctionSupportedValue(new FixedValue(JToken.Parse($"\"{a}\"")))));
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var argumentValues = GetArgumentStrings(arguments, currentToken, dataContext, options);
            var argumentSettings = GetExecutionSettings(argumentValues);
            var result = GetToken(argumentSettings.dateSelection, argumentSettings.format, options.Logger);
            return new JLioExecutionResult(result.Success, result.JToken);
        }


        private DateTimeConversionResult GetToken(string dateSelection, string format, IJLioExecutionLogger logger)
        {
            var datetimeConversionResult = GetDateTime(dateSelection, logger);
            if (!datetimeConversionResult.Success)
            {
                // no datetime indication so the first argument could be the format
                format = dateSelection;
                datetimeConversionResult.Success = true;
            }

            try
            {
                var settings = new JsonSerializerSettings { DateFormatString = format };
                datetimeConversionResult.JToken =
                    JToken.Parse(JsonConvert.SerializeObject(datetimeConversionResult.DateTime, settings));
            }
            catch (Exception)
            {
                logger.Log(LogLevel.Error, JLioConstants.FunctionExecution,
                    $"faulty conversion of the date due to the formatting {format}.");
                datetimeConversionResult.JToken = JToken.Parse(JsonConvert.SerializeObject(DateTime.Now));
                datetimeConversionResult.Success = false;
            }

            return datetimeConversionResult;
        }

        private DateTimeConversionResult GetDateTime(string dateSelection, IJLioExecutionLogger logger)
        {
            switch (dateSelection)
            {
                case "now":
                    return new DateTimeConversionResult { DateTime = DateTime.Now, Success = true };
                case "UTC":
                    return new DateTimeConversionResult { DateTime = DateTime.UtcNow, Success = true };
                case "startOfDay":
                    return new DateTimeConversionResult { DateTime = DateTime.Now.Date, Success = true };
                case "startOfDayUTC":
                    return new DateTimeConversionResult { DateTime = DateTime.UtcNow.Date, Success = true };
                default:
                    logger.Log(LogLevel.Information, JLioConstants.FunctionExecution,
                        $"unknown datetime indication {dateSelection}, assuming this is a datetime format");
                    return new DateTimeConversionResult { DateTime = DateTime.Now, Success = false };
            }
        }

        private static (string dateSelection, string format) GetExecutionSettings(List<string> argumentValues)
        {
            var dateSelection = "now";
            var format = "yyyy-MM-ddTHH:mm:ss.fffZ";
            if (argumentValues.Count > 0) dateSelection = argumentValues[0].Trim(JLioConstants.StringIndicator);
            if (argumentValues.Count > 1) format = argumentValues[1].Trim(JLioConstants.StringIndicator);

            return (dateSelection, format);
        }

        internal class DateTimeConversionResult
        {
            public DateTime DateTime { get; set; }
            public JToken JToken { get; set; }
            public bool Success { get; set; }
        }
    }
}