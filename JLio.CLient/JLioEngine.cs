using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Client;

/// <summary>
/// JLio execution engine that encapsulates parse options and execution context.
/// Provides a non-singleton approach for running JLio transformations with specific configurations.
/// </summary>
public class JLioEngine
{
    private readonly IParseOptions parseOptions;
    private readonly IExecutionContext executionContext;

    /// <summary>
    /// Creates a new JLio engine with the specified parse options and execution context.
    /// </summary>
    /// <param name="parseOptions">Configuration for parsing JLio scripts</param>
    /// <param name="executionContext">Context for executing JLio scripts</param>
    public JLioEngine(IParseOptions parseOptions, IExecutionContext executionContext)
    {
        this.parseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions));
        this.executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
    }

    /// <summary>
    /// Parse a JLio script using this engine's parse options.
    /// </summary>
    /// <param name="script">The JLio script to parse</param>
    /// <returns>Parsed JLio script ready for execution</returns>
    public JLioScript Parse(string script)
    {
        return JLioConvert.Parse(script, parseOptions);
    }

    /// <summary>
    /// Execute a JLio script using this engine's execution context.
    /// </summary>
    /// <param name="script">The parsed JLio script to execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <returns>Execution result containing success status and transformed data</returns>
    public JLioExecutionResult Execute(JLioScript script, JToken data)
    {
        return script.Execute(data, executionContext);
    }

    /// <summary>
    /// Parse and execute a JLio script in one operation.
    /// </summary>
    /// <param name="script">The JLio script to parse and execute</param>
    /// <param name="data">The JSON data to transform</param>
    /// <returns>Execution result containing success status and transformed data</returns>
    public JLioExecutionResult ParseAndExecute(string script, JToken data)
    {
        var parsedScript = Parse(script);
        return Execute(parsedScript, data);
    }
}