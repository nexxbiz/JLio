using System;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Parse : FunctionBase
    {
        public Parse()
        {
        }

        public Parse(string path)
        {
            arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{path}\""))));
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            if (arguments.Any()) return ExecuteWithArguments(currentToken, dataContext, options);
            return ExecuteWithoutArguments(currentToken, options);
        }

        private JLioExecutionResult ExecuteWithArguments(JToken currentToken, JToken dataContext,
            IExecutionOptions options)
        {
            var values = GetArgumentStrings(arguments, currentToken, dataContext, options);
            return !values.Any()
                ? new JLioExecutionResult(false, JValue.CreateNull())
                : TryParse(values.First(), options);
        }

        private JLioExecutionResult ExecuteWithoutArguments(JToken currentToken, IExecutionOptions options)
        {
            if (currentToken.Type == JTokenType.String)
                return TryParse(currentToken.Value<string>() ?? currentToken.ToString(), options);

            options.Logger.Log(LogLevel.Warning, CoreConstants.FunctionExecution,
                $"Function {FunctionName} only works on type string. Current type = {currentToken.Type}!");
            return new JLioExecutionResult(true, currentToken);
        }

        private JLioExecutionResult TryParse(string value, IExecutionOptions options)
        {
            try
            {
                return new JLioExecutionResult(true, JToken.Parse(value));
            }
            catch (Exception e)
            {
                options.Logger.Log(LogLevel.Warning, CoreConstants.FunctionExecution,
                    $"Function {FunctionName} failed: unable to parse {value}");

                return new JLioExecutionResult(false, value);
            }
        }
    }
}