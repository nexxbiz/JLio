using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core;

namespace JLio.Client;

/// <summary>
/// Advanced builder for creating JLio engines that supports loading different versions of extension packages.
/// Enables true side-by-side deployment of different NuGet package versions.
/// </summary>
public class JLioVersionedEngineBuilder
{
    private readonly List<Type> commandTypes = new();
    private readonly List<Type> functionTypes = new();
    private readonly List<Action<IParseOptions>> parseConfigurators = new();
    private readonly List<Action<IExecutionContext>> executionConfigurators = new();
    private readonly Dictionary<string, AssemblyLoadContext> packageContexts = new();
    private readonly Dictionary<string, string> packageVersions = new();

    /// <summary>
    /// Add a command type to the engine configuration.
    /// </summary>
    /// <typeparam name="TCommand">The command type to register</typeparam>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithCommand<TCommand>() where TCommand : ICommand
    {
        commandTypes.Add(typeof(TCommand));
        return this;
    }

    /// <summary>
    /// Add a function type to the engine configuration.
    /// </summary>
    /// <typeparam name="TFunction">The function type to register</typeparam>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithFunction<TFunction>() where TFunction : IFunction
    {
        functionTypes.Add(typeof(TFunction));
        return this;
    }

    /// <summary>
    /// Load a specific version of an extension package from a directory.
    /// This enables side-by-side deployment of different package versions.
    /// </summary>
    /// <param name="packageName">Name identifier for the package (e.g., "JLio.Extensions.Math")</param>
    /// <param name="version">Version identifier (e.g., "2.0.0")</param>
    /// <param name="assemblyPath">Path to the assembly file</param>
    /// <param name="registerAction">Action to register the package's extensions</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithVersionedPackage(
        string packageName, 
        string version, 
        string assemblyPath,
        Action<IParseOptions, Assembly> registerAction)
    {
        var contextName = $"{packageName}_{version}";
        
        // Create isolated AssemblyLoadContext for this package version
        var loadContext = new AssemblyLoadContext(contextName, isCollectible: true);
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
        
        packageContexts[contextName] = loadContext;
        packageVersions[contextName] = version;

        parseConfigurators.Add(options => 
        {
            try 
            {
                registerAction(options, assembly);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to register package {packageName} version {version}", ex);
            }
        });

        return this;
    }

    /// <summary>
    /// Load Math extensions from a specific version.
    /// </summary>
    /// <param name="version">Version to load (e.g., "1.0.0", "2.0.0")</param>
    /// <param name="assemblyPath">Optional custom path to the assembly</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithMathExtensions(string version = "default", string assemblyPath = null)
    {
        if (version == "default")
        {
            // Use standard reflection loading for default/current version
            parseConfigurators.Add(options => 
            {
                try 
                {
                    var mathAssembly = Assembly.Load("JLio.Extensions.Math");
                    var registerMathPackType = mathAssembly.GetType("JLio.Extensions.Math.RegisterMathPack");
                    var registerMathMethod = registerMathPackType?.GetMethod("RegisterMath", new[] { typeof(IParseOptions) });
                    registerMathMethod?.Invoke(null, new object[] { options });
                }
                catch (Exception)
                {
                    // Silently fail if assembly is not available
                }
            });
        }
        else
        {
            if (string.IsNullOrEmpty(assemblyPath))
                assemblyPath = Path.Combine(AppContext.BaseDirectory, "packages", $"JLio.Extensions.Math.{version}", "JLio.Extensions.Math.dll");

            WithVersionedPackage("JLio.Extensions.Math", version, assemblyPath, (options, assembly) =>
            {
                var registerMathPackType = assembly.GetType("JLio.Extensions.Math.RegisterMathPack");
                var registerMathMethod = registerMathPackType?.GetMethod("RegisterMath", new[] { typeof(IParseOptions) });
                registerMathMethod?.Invoke(null, new object[] { options });
            });
        }
        return this;
    }

    /// <summary>
    /// Load Text extensions from a specific version.
    /// </summary>
    /// <param name="version">Version to load</param>
    /// <param name="assemblyPath">Optional custom path to the assembly</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithTextExtensions(string version = "default", string assemblyPath = null)
    {
        if (version == "default")
        {
            parseConfigurators.Add(options => 
            {
                try 
                {
                    var textAssembly = Assembly.Load("JLio.Extensions.Text");
                    var registerTextPackType = textAssembly.GetType("JLio.Extensions.Text.RegisterTextPack");
                    var registerTextMethod = registerTextPackType?.GetMethod("RegisterText", new[] { typeof(IParseOptions) });
                    registerTextMethod?.Invoke(null, new object[] { options });
                }
                catch (Exception)
                {
                    // Silently fail if assembly is not available
                }
            });
        }
        else
        {
            if (string.IsNullOrEmpty(assemblyPath))
                assemblyPath = Path.Combine(AppContext.BaseDirectory, "packages", $"JLio.Extensions.Text.{version}", "JLio.Extensions.Text.dll");

            WithVersionedPackage("JLio.Extensions.Text", version, assemblyPath, (options, assembly) =>
            {
                var registerTextPackType = assembly.GetType("JLio.Extensions.Text.RegisterTextPack");
                var registerTextMethod = registerTextPackType?.GetMethod("RegisterText", new[] { typeof(IParseOptions) });
                registerTextMethod?.Invoke(null, new object[] { options });
            });
        }
        return this;
    }

    /// <summary>
    /// Load ETL extensions from a specific version.
    /// </summary>
    /// <param name="version">Version to load</param>
    /// <param name="assemblyPath">Optional custom path to the assembly</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithETLExtensions(string version = "default", string assemblyPath = null)
    {
        if (version == "default")
        {
            parseConfigurators.Add(options => 
            {
                try 
                {
                    var etlAssembly = Assembly.Load("JLio.Extensions.ETL");
                    var registerETLPackType = etlAssembly.GetType("JLio.Extensions.ETL.RegisterETLPack");
                    var registerETLMethod = registerETLPackType?.GetMethod("RegisterETL", new[] { typeof(IParseOptions) });
                    registerETLMethod?.Invoke(null, new object[] { options });
                }
                catch (Exception)
                {
                    // Silently fail if assembly is not available
                }
            });
        }
        else
        {
            if (string.IsNullOrEmpty(assemblyPath))
                assemblyPath = Path.Combine(AppContext.BaseDirectory, "packages", $"JLio.Extensions.ETL.{version}", "JLio.Extensions.ETL.dll");

            WithVersionedPackage("JLio.Extensions.ETL", version, assemblyPath, (options, assembly) =>
            {
                var registerETLPackType = assembly.GetType("JLio.Extensions.ETL.RegisterETLPack");
                var registerETLMethod = registerETLPackType?.GetMethod("RegisterETL", new[] { typeof(IParseOptions) });
                registerETLMethod?.Invoke(null, new object[] { options });
            });
        }
        return this;
    }

    /// <summary>
    /// Add all core commands (Add, Set, Put, Remove, Copy, Move).
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithCoreCommands()
    {
        return this
            .WithCommand<Commands.Add>()
            .WithCommand<Commands.Set>()
            .WithCommand<Commands.Put>()
            .WithCommand<Commands.Remove>()
            .WithCommand<Commands.Copy>()
            .WithCommand<Commands.Move>();
    }

    /// <summary>
    /// Add all advanced commands (Compare, Merge, DecisionTable, IfElse).
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithAdvancedCommands()
    {
        return this
            .WithCommand<Commands.Advanced.Compare>()
            .WithCommand<Commands.Advanced.Merge>()
            .WithCommand<Commands.DecisionTable>()
            .WithCommand<Commands.IfElse>();
    }

    /// <summary>
    /// Add all core functions (Datetime, Partial, Promote, Fetch, Indirect, ScriptPath).
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder WithCoreFunctions()
    {
        return this
            .WithFunction<Functions.Datetime>()
            .WithFunction<Functions.Partial>()
            .WithFunction<Functions.Promote>()
            .WithFunction<Functions.Fetch>()
            .WithFunction<Functions.Indirect>()
            .WithFunction<Functions.ScriptPath>();
    }

    /// <summary>
    /// Add custom configuration for parse options.
    /// </summary>
    /// <param name="configurator">Action to configure parse options</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder ConfigureParsing(Action<IParseOptions> configurator)
    {
        parseConfigurators.Add(configurator);
        return this;
    }

    /// <summary>
    /// Add custom configuration for execution context.
    /// </summary>
    /// <param name="configurator">Action to configure execution context</param>
    /// <returns>This builder for method chaining</returns>
    public JLioVersionedEngineBuilder ConfigureExecution(Action<IExecutionContext> configurator)
    {
        executionConfigurators.Add(configurator);
        return this;
    }

    /// <summary>
    /// Build the JLio engine with the configured options.
    /// </summary>
    /// <returns>A new versioned JLio engine instance</returns>
    public JLioVersionedEngine Build()
    {
        var parseOptions = CreateParseOptions();
        var executionContext = CreateExecutionContext();

        return new JLioVersionedEngine(parseOptions, executionContext, packageContexts, packageVersions);
    }

    private IParseOptions CreateParseOptions()
    {
        var commandsProvider = new CommandsProvider();
        var functionsProvider = new FunctionsProvider();

        // Register commands
        foreach (var commandType in commandTypes)
        {
            commandsProvider.Register(commandType);
        }

        // Register functions  
        foreach (var functionType in functionTypes)
        {
            functionsProvider.Register(functionType);
        }

        // Create ParseOptions using the same pattern as CreateDefault
        var options = new ParseOptions
        {
            JLioCommandConverter = new CommandConverter(commandsProvider),
            JLioFunctionConverter = new FunctionConverter(functionsProvider)
        };

        // Use reflection to set the private properties
        var functionsProviderBackingField = typeof(ParseOptions).GetField("<FunctionsProvider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        functionsProviderBackingField?.SetValue(options, functionsProvider);

        var commandsProviderBackingField = typeof(ParseOptions).GetField("<CommandsProvider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        commandsProviderBackingField?.SetValue(options, commandsProvider);

        // Set the default function converter for FixedValue (for backward compatibility)
        FixedValue.DefaultFunctionConverter = (FunctionConverter)options.JLioFunctionConverter;

        // Apply custom configurations
        foreach (var configurator in parseConfigurators)
        {
            configurator(options);
        }

        return options;
    }

    private IExecutionContext CreateExecutionContext()
    {
        var context = ExecutionContext.CreateDefault();

        // Apply custom configurations
        foreach (var configurator in executionConfigurators)
        {
            configurator(context);
        }

        return context;
    }
}