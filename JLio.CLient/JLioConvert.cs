using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Client;

public static class JLioConvert
{
    private static readonly Lazy<JLioEngine> DefaultEngine = new(() => 
        JLioEngineConfigurations.CreateLatest().Build());

    /// <summary>
    /// Parse a JLio script using default configuration.
    /// </summary>
    /// <param name="script">The JLio script to parse</param>
    /// <returns>Parsed JLio script</returns>
    public static JLioScript Parse(string script)
    {
        return Parse(script, ParseOptions.CreateDefault());
    }

    /// <summary>
    /// Parse a JLio script using custom parse options.
    /// </summary>
    /// <param name="script">The JLio script to parse</param>
    /// <param name="options">Custom parse options</param>
    /// <returns>Parsed JLio script</returns>
    public static JLioScript Parse(string script, IParseOptions options)
    {
        if (string.IsNullOrEmpty(script)) return new JLioScript();
        var converters = new[] {options.JLioCommandConverter, options.JLioFunctionConverter};
        return JsonConvert.DeserializeObject<JLioScript>(script, converters);
    }

    /// <summary>
    /// Serialize a JLio script using default configuration.
    /// </summary>
    /// <param name="script">The JLio script to serialize</param>
    /// <returns>JSON representation of the script</returns>
    public static string Serialize(JLioScript script)
    {
        return Serialize(script, ParseOptions.CreateDefault());
    }

    /// <summary>
    /// Serialize a JLio script using custom parse options.
    /// </summary>
    /// <param name="script">The JLio script to serialize</param>
    /// <param name="options">Custom parse options</param>
    /// <returns>JSON representation of the script</returns>
    public static string Serialize(JLioScript script, IParseOptions options)
    {
        var converters = new[] {options.JLioCommandConverter, options.JLioFunctionConverter};
        return JsonConvert.SerializeObject(script, Formatting.Indented, converters);
    }

    /// <summary>
    /// Parse and execute a JLio script using the default engine.
    /// This is a new convenience method that uses the engine architecture.
    /// </summary>
    /// <param name="script">The JLio script to parse and execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult ParseAndExecute(string script, JToken data)
    {
        return DefaultEngine.Value.ParseAndExecute(script, data);
    }

    /// <summary>
    /// Parse and execute a JLio script using the default engine with custom parse options.
    /// </summary>
    /// <param name="script">The JLio script to parse and execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <param name="parseOptions">Custom parse options</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult ParseAndExecute(string script, JToken data, IParseOptions parseOptions)
    {
        var engine = new JLioEngine(parseOptions, ExecutionContext.CreateDefault());
        return engine.ParseAndExecute(script, data);
    }

    /// <summary>
    /// Parse and execute a JLio script using the default engine with custom execution context.
    /// </summary>
    /// <param name="script">The JLio script to parse and execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <param name="executionContext">Custom execution context</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult ParseAndExecute(string script, JToken data, IExecutionContext executionContext)
    {
        var engine = new JLioEngine(ParseOptions.CreateDefault(), executionContext);
        return engine.ParseAndExecute(script, data);
    }

    /// <summary>
    /// Parse and execute a JLio script using custom options and context.
    /// </summary>
    /// <param name="script">The JLio script to parse and execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <param name="parseOptions">Custom parse options</param>
    /// <param name="executionContext">Custom execution context</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult ParseAndExecute(string script, JToken data, IParseOptions parseOptions, IExecutionContext executionContext)
    {
        var engine = new JLioEngine(parseOptions, executionContext);
        return engine.ParseAndExecute(script, data);
    }
}