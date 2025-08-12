using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Client;

/// <summary>
/// JLio execution engine that supports multiple versions of extension packages loaded side-by-side.
/// This engine manages its own AssemblyLoadContexts for true package isolation.
/// </summary>
public class JLioVersionedEngine : IDisposable
{
    private readonly IParseOptions parseOptions;
    private readonly IExecutionContext executionContext;
    private readonly Dictionary<string, AssemblyLoadContext> packageContexts;
    private readonly Dictionary<string, string> packageVersions;
    private bool disposed = false;

    /// <summary>
    /// Creates a new versioned JLio engine with the specified configurations and package contexts.
    /// </summary>
    /// <param name="parseOptions">Configuration for parsing JLio scripts</param>
    /// <param name="executionContext">Context for executing JLio scripts</param>
    /// <param name="packageContexts">Assembly load contexts for different package versions</param>
    /// <param name="packageVersions">Version information for loaded packages</param>
    internal JLioVersionedEngine(
        IParseOptions parseOptions, 
        IExecutionContext executionContext,
        Dictionary<string, AssemblyLoadContext> packageContexts,
        Dictionary<string, string> packageVersions)
    {
        this.parseOptions = parseOptions ?? throw new ArgumentNullException(nameof(parseOptions));
        this.executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        this.packageContexts = packageContexts ?? new Dictionary<string, AssemblyLoadContext>();
        this.packageVersions = packageVersions ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Get information about loaded package versions.
    /// </summary>
    /// <returns>Dictionary of package name to version mapping</returns>
    public IReadOnlyDictionary<string, string> GetLoadedPackageVersions()
    {
        return packageVersions;
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

    /// <summary>
    /// Dispose of the engine and unload all package contexts.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            foreach (var context in packageContexts.Values)
            {
                try
                {
                    if (context.IsCollectible)
                    {
                        context.Unload();
                    }
                }
                catch
                {
                    // Ignore errors during unload
                }
            }
            packageContexts.Clear();
            disposed = true;
        }
    }

    ~JLioVersionedEngine()
    {
        Dispose(false);
    }
}