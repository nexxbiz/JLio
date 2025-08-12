using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Client;

/// <summary>
/// Predefined JLio engine configurations for different feature sets and versions.
/// Provides easy access to commonly used engine setups.
/// </summary>
public static class JLioEngineConfigurations
{
    /// <summary>
    /// Create a V1 engine builder with minimal core functionality.
    /// Includes: Core commands (Add, Set, Put, Remove, Copy, Move) + Core functions.
    /// </summary>
    /// <returns>Engine builder configured for V1 feature set</returns>
    public static JLioEngineBuilder CreateV1()
    {
        return new JLioEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions();
    }

    /// <summary>
    /// Create a V2 engine builder with advanced functionality.
    /// Includes: V1 features + Advanced commands (Compare, Merge, DecisionTable, IfElse).
    /// </summary>
    /// <returns>Engine builder configured for V2 feature set</returns>
    public static JLioEngineBuilder CreateV2()
    {
        return CreateV1()
            .WithAdvancedCommands();
    }

    /// <summary>
    /// Create a V3 engine builder with full functionality including ETL capabilities.
    /// Includes: V2 features + all extension packs.
    /// </summary>
    /// <returns>Engine builder configured for V3 feature set</returns>
    public static JLioEngineBuilder CreateV3()
    {
        return CreateV2()
            .WithAllExtensions();
    }

    /// <summary>
    /// Create an engine builder with the latest available feature set.
    /// Currently equivalent to V3.
    /// </summary>
    /// <returns>Engine builder configured with latest features</returns>
    public static JLioEngineBuilder CreateLatest()
    {
        return CreateV3();
    }

    /// <summary>
    /// Create an engine builder with only the most basic functionality.
    /// Includes: Add, Set, Remove commands + Fetch, Datetime functions.
    /// </summary>
    /// <returns>Engine builder configured for minimal functionality</returns>
    public static JLioEngineBuilder CreateMinimal()
    {
        return new JLioEngineBuilder()
            .WithCommand<Commands.Add>()
            .WithCommand<Commands.Set>()
            .WithCommand<Commands.Remove>()
            .WithFunction<Functions.Fetch>()
            .WithFunction<Functions.Datetime>();
    }

    /// <summary>
    /// Create an engine builder optimized for data transformation scenarios.
    /// Includes: All commands + Math and Text extensions.
    /// </summary>
    /// <returns>Engine builder configured for data transformation</returns>
    public static JLioEngineBuilder CreateForDataTransformation()
    {
        return CreateV2()
            .WithMathExtensions()
            .WithTextExtensions();
    }

    /// <summary>
    /// Create an engine builder optimized for ETL scenarios.
    /// Includes: All features with focus on ETL capabilities.
    /// </summary>
    /// <returns>Engine builder configured for ETL operations</returns>
    public static JLioEngineBuilder CreateForETL()
    {
        return CreateV2()
            .WithETLExtensions()
            .WithMathExtensions();
    }
}

/// <summary>
/// Registry for managing named JLio engines with specific configurations.
/// Enables centralized engine management and easy retrieval by name.
/// </summary>
public static class JLioNamedEngines
{
    private static readonly ConcurrentDictionary<string, JLioEngine> RegisteredEngines = new();
    private static readonly ConcurrentDictionary<string, JLioVersionedEngine> RegisteredVersionedEngines = new();
    private static readonly ConcurrentDictionary<string, Func<JLioEngine>> EngineFactories = new();
    private static readonly ConcurrentDictionary<string, Func<JLioVersionedEngine>> VersionedEngineFactories = new();

    static JLioNamedEngines()
    {
        RegisterDefaultEngines();
    }

    /// <summary>
    /// Register a standard engine with a specific name and configuration.
    /// </summary>
    /// <param name="name">Unique name for the engine</param>
    /// <param name="builderFunc">Function that configures the engine builder</param>
    /// <param name="singleton">If true, creates one instance and reuses it. If false, creates new instance each time.</param>
    public static void Register(string name, Func<JLioEngineBuilder, JLioEngine> builderFunc, bool singleton = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Engine name cannot be null or empty", nameof(name));

        var factory = () => builderFunc(new JLioEngineBuilder());

        if (singleton)
        {
            var engine = factory();
            RegisteredEngines.AddOrUpdate(name, engine, (key, old) => engine);
        }
        else
        {
            EngineFactories.AddOrUpdate(name, factory, (key, old) => factory);
        }
    }

    /// <summary>
    /// Register a versioned engine with a specific name and configuration.
    /// </summary>
    /// <param name="name">Unique name for the engine</param>
    /// <param name="builderFunc">Function that configures the versioned engine builder</param>
    /// <param name="singleton">If true, creates one instance and reuses it. If false, creates new instance each time.</param>
    public static void RegisterVersioned(string name, Func<JLioVersionedEngineBuilder, JLioVersionedEngine> builderFunc, bool singleton = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Engine name cannot be null or empty", nameof(name));

        var factory = () => builderFunc(new JLioVersionedEngineBuilder());

        if (singleton)
        {
            var engine = factory();
            RegisteredVersionedEngines.AddOrUpdate(name, engine, (key, old) => engine);
        }
        else
        {
            VersionedEngineFactories.AddOrUpdate(name, factory, (key, old) => factory);
        }
    }

    /// <summary>
    /// Get a standard engine by name.
    /// </summary>
    /// <param name="name">Name of the engine</param>
    /// <returns>The engine instance</returns>
    /// <exception cref="KeyNotFoundException">Thrown when engine name is not found</exception>
    public static JLioEngine Get(string name)
    {
        // Try singleton first
        if (RegisteredEngines.TryGetValue(name, out var singletonEngine))
            return singletonEngine;

        // Try factory
        if (EngineFactories.TryGetValue(name, out var factory))
            return factory();

        throw new KeyNotFoundException($"No engine registered with name '{name}'. Available engines: {string.Join(", ", GetNames())}");
    }

    /// <summary>
    /// Get a versioned engine by name.
    /// </summary>
    /// <param name="name">Name of the versioned engine</param>
    /// <returns>The versioned engine instance</returns>
    /// <exception cref="KeyNotFoundException">Thrown when engine name is not found</exception>
    public static JLioVersionedEngine GetVersioned(string name)
    {
        // Try singleton first
        if (RegisteredVersionedEngines.TryGetValue(name, out var singletonEngine))
            return singletonEngine;

        // Try factory
        if (VersionedEngineFactories.TryGetValue(name, out var factory))
            return factory();

        throw new KeyNotFoundException($"No versioned engine registered with name '{name}'. Available versioned engines: {string.Join(", ", GetVersionedNames())}");
    }

    /// <summary>
    /// Try to get a standard engine by name.
    /// </summary>
    /// <param name="name">Name of the engine</param>
    /// <param name="engine">The engine instance if found</param>
    /// <returns>True if engine was found, false otherwise</returns>
    public static bool TryGet(string name, out JLioEngine engine)
    {
        try
        {
            engine = Get(name);
            return true;
        }
        catch (KeyNotFoundException)
        {
            engine = null;
            return false;
        }
    }

    /// <summary>
    /// Try to get a versioned engine by name.
    /// </summary>
    /// <param name="name">Name of the versioned engine</param>
    /// <param name="engine">The versioned engine instance if found</param>
    /// <returns>True if engine was found, false otherwise</returns>
    public static bool TryGetVersioned(string name, out JLioVersionedEngine engine)
    {
        try
        {
            engine = GetVersioned(name);
            return true;
        }
        catch (KeyNotFoundException)
        {
            engine = null;
            return false;
        }
    }

    /// <summary>
    /// Check if an engine is registered with the given name.
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <returns>True if engine exists, false otherwise</returns>
    public static bool HasEngine(string name)
    {
        return RegisteredEngines.ContainsKey(name) || EngineFactories.ContainsKey(name);
    }

    /// <summary>
    /// Check if a versioned engine is registered with the given name.
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <returns>True if versioned engine exists, false otherwise</returns>
    public static bool HasVersionedEngine(string name)
    {
        return RegisteredVersionedEngines.ContainsKey(name) || VersionedEngineFactories.ContainsKey(name);
    }

    /// <summary>
    /// Get all registered standard engine names.
    /// </summary>
    /// <returns>Collection of engine names</returns>
    public static IEnumerable<string> GetNames()
    {
        var names = new List<string>();
        names.AddRange(RegisteredEngines.Keys);
        names.AddRange(EngineFactories.Keys);
        return names;
    }

    /// <summary>
    /// Get all registered versioned engine names.
    /// </summary>
    /// <returns>Collection of versioned engine names</returns>
    public static IEnumerable<string> GetVersionedNames()
    {
        var names = new List<string>();
        names.AddRange(RegisteredVersionedEngines.Keys);
        names.AddRange(VersionedEngineFactories.Keys);
        return names;
    }

    /// <summary>
    /// Execute a JLio script using a named engine.
    /// </summary>
    /// <param name="engineName">Name of the engine to use</param>
    /// <param name="script">JLio script to execute</param>
    /// <param name="data">Data to transform</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult Execute(string engineName, string script, JToken data)
    {
        var engine = Get(engineName);
        return engine.ParseAndExecute(script, data);
    }

    /// <summary>
    /// Execute a JLio script using a named versioned engine.
    /// </summary>
    /// <param name="engineName">Name of the versioned engine to use</param>
    /// <param name="script">JLio script to execute</param>
    /// <param name="data">Data to transform</param>
    /// <returns>Execution result</returns>
    public static JLioExecutionResult ExecuteVersioned(string engineName, string script, JToken data)
    {
        var engine = GetVersioned(engineName);
        return engine.ParseAndExecute(script, data);
    }

    /// <summary>
    /// Remove an engine from the registry.
    /// </summary>
    /// <param name="name">Name of the engine to remove</param>
    /// <returns>True if engine was removed, false if not found</returns>
    public static bool Remove(string name)
    {
        var removedSingleton = RegisteredEngines.TryRemove(name, out var engine);
        var removedFactory = EngineFactories.TryRemove(name, out _);
        
        // Dispose if it's disposable
        if (engine is IDisposable disposable)
            disposable.Dispose();

        return removedSingleton || removedFactory;
    }

    /// <summary>
    /// Remove a versioned engine from the registry.
    /// </summary>
    /// <param name="name">Name of the versioned engine to remove</param>
    /// <returns>True if engine was removed, false if not found</returns>
    public static bool RemoveVersioned(string name)
    {
        var removedSingleton = RegisteredVersionedEngines.TryRemove(name, out var engine);
        var removedFactory = VersionedEngineFactories.TryRemove(name, out _);
        
        // Dispose the versioned engine
        engine?.Dispose();

        return removedSingleton || removedFactory;
    }

    /// <summary>
    /// Clear all registered engines and dispose disposable ones.
    /// </summary>
    public static void Clear()
    {
        // Dispose versioned engines
        foreach (var engine in RegisteredVersionedEngines.Values)
        {
            engine?.Dispose();
        }

        // Dispose standard engines that are disposable
        foreach (var engine in RegisteredEngines.Values)
        {
            if (engine is IDisposable disposable)
                disposable.Dispose();
        }

        RegisteredEngines.Clear();
        RegisteredVersionedEngines.Clear();
        EngineFactories.Clear();
        VersionedEngineFactories.Clear();
        
        // Re-register defaults
        RegisterDefaultEngines();
    }

    private static void RegisterDefaultEngines()
    {
        // Register default engine configurations
        Register("minimal", builder => JLioEngineConfigurations.CreateMinimal().Build());
        Register("v1", builder => JLioEngineConfigurations.CreateV1().Build());
        Register("v2", builder => JLioEngineConfigurations.CreateV2().Build());
        Register("v3", builder => JLioEngineConfigurations.CreateV3().Build());
        Register("latest", builder => JLioEngineConfigurations.CreateLatest().Build());
        Register("data-transformation", builder => JLioEngineConfigurations.CreateForDataTransformation().Build());
        Register("etl", builder => JLioEngineConfigurations.CreateForETL().Build());
        Register("default", builder => JLioEngineConfigurations.CreateLatest().Build());
    }
}