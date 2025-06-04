using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace JLio.Core;

public class FixedValue : IFunction
{
    public FixedValue(JToken value, FunctionConverter functionConverter)
    {
        Value = value;
        this.FunctionConverter = functionConverter;
    }

    public JToken Value { get; }

    public string FunctionName => "FixedValue";

    public FunctionConverter FunctionConverter { get; set; }

    public IFunction SetArguments(Arguments newArguments)
    {
        return this;
    }

    public JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Value.Type == JTokenType.String) return HandleString(currentToken, dataContext, context);
        if (FunctionConverter != null && (Value.Type == JTokenType.Object || Value.Type == JTokenType.Array)) return ProcessToken(Value, currentToken, dataContext, context);
        return new JLioFunctionResult(true, Value);
    }

    private JLioFunctionResult ProcessToken(JToken token, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var clone = token.DeepClone();
        if (token.Type == JTokenType.Object) ProcessObject((JObject)clone, currentToken, dataContext, context);
        if (token.Type == JTokenType.Array) ProcessArray((JArray)clone, currentToken, dataContext, context);
        return new JLioFunctionResult(true, clone);
    }

    private void ProcessObject(JObject obj, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        foreach (var prop in obj.Properties().ToList())
        {
            prop.Value = ProcessValue(prop.Value, currentToken, dataContext, context);
        }
    }

    private void ProcessArray(JArray array, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        for (int i = 0; i < array.Count; i++)
            array[i] = ProcessValue(array[i], currentToken, dataContext, context);
    }

    private JToken ProcessValue(JToken value, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (IsFunction(value)) return ExecuteFunction(value.ToString(), currentToken, dataContext, context);
        if (value.Type == JTokenType.Object || value.Type == JTokenType.Array) { ProcessToken(value, currentToken, dataContext, context); }
        ;
        return value;
    }

    private bool IsFunction(JToken token)
    {
        return token.Type == JTokenType.String && token.ToString().StartsWith("=");
    }

    private JToken ExecuteFunction(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (FunctionConverter == null)
        {
            context.LogError(CoreConstants.FunctionExecution, "FunctionsConverter is not set for FixedValue execution.");
            return JValue.CreateString(expression); // or throw an exception
        }
        var function = FunctionConverter.ParseString(expression);
        var result = function.GetValue(currentToken, dataContext, context); // Execute the function with the current context
        if (!result.Success)
            return JValue.CreateString(expression);
        return result.Data.FirstOrDefault(); // Return the result of the function execution   

    }

    //traverse if it is a object or array 
    // properties that are a string and start with =
    // try parse to fucntion and exectute function


    public string ToScript()
    {
        return Value.ToString();
    }

    private JLioFunctionResult HandleString(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var stringValue = Value.ToString();

        if (stringValue.StartsWith(context.ItemsFetcher.CurrentItemPathIndicator,
            StringComparison.InvariantCulture))
            return new JLioFunctionResult(true,
                context.ItemsFetcher.SelectTokens(
                    $"{context.ItemsFetcher.RootPathIndicator}{stringValue.Substring(context.ItemsFetcher.CurrentItemPathIndicator.Length)}"
                    , currentToken));
        if (stringValue.StartsWith(context.ItemsFetcher.RootPathIndicator, StringComparison.InvariantCulture))
            return new JLioFunctionResult(true, context.ItemsFetcher.SelectTokens(stringValue, dataContext));
        return new JLioFunctionResult(true, Value);
    }
}