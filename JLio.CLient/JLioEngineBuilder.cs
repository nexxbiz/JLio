using System;
using System.Collections.Generic;
using System.Reflection;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core;

namespace JLio.Client;

/// <summary>
/// Builder for creating JLio engines with custom configurations.
/// Provides a fluent API for specifying commands, functions, and settings.
/// </summary>
public class JLioEngineBuilder
{
    private readonly List<Type> commandTypes = new();
    private readonly List<Type> functionTypes = new();
    private readonly List<Action<IParseOptions>> parseConfigurators = new();
    private readonly List<Action<IExecutionContext>> executionConfigurators = new();

    /// <summary>
    /// Add a command type to the engine configuration.
    /// </summary>
    /// <typeparam name="TCommand">The command type to register</typeparam>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithCommand<TCommand>() where TCommand : ICommand
    {
        commandTypes.Add(typeof(TCommand));
        return this;
    }

    /// <summary>
    /// Add a function type to the engine configuration.
    /// </summary>
    /// <typeparam name="TFunction">The function type to register</typeparam>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithFunction<TFunction>() where TFunction : IFunction
    {
        functionTypes.Add(typeof(TFunction));
        return this;
    }

    /// <summary>
    /// Add all core commands (Add, Set, Put, Remove, Copy, Move).
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithCoreCommands()
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
    public JLioEngineBuilder WithAdvancedCommands()
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
    public JLioEngineBuilder WithCoreFunctions()
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
    /// Add math extension functions using the extension pack.
    /// Note: This requires a reference to JLio.Extensions.Math.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithMathExtensions()
    {
        parseConfigurators.Add(options => 
        {
            try 
            {
                // Use reflection to call RegisterMath extension method
                var mathAssembly = Assembly.Load("JLio.Extensions.Math");
                var registerMathPackType = mathAssembly.GetType("JLio.Extensions.Math.RegisterMathPack");
                var registerMathMethod = registerMathPackType?.GetMethod("RegisterMath", new[] { typeof(IParseOptions) });
                registerMathMethod?.Invoke(null, new object[] { options });
            }
            catch (Exception)
            {
                // Silently fail if assembly is not available - allows for optional dependencies
            }
        });
        return this;
    }

    /// <summary>
    /// Add text extension functions.
    /// Note: This requires a reference to JLio.Extensions.Text.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithTextExtensions()
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
        return this;
    }

    /// <summary>
    /// Add ETL extension functions and commands.
    /// Note: This requires a reference to JLio.Extensions.ETL.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithETLExtensions()
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
        return this;
    }

    /// <summary>
    /// Add JSchema extension functions.
    /// Note: This requires a reference to JLio.Extensions.JSchema.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithJSchemaExtensions()
    {
        parseConfigurators.Add(options => 
        {
            try 
            {
                var jschemaAssembly = Assembly.Load("JLio.Extensions.JSchema");
                var registerJSchemaPackType = jschemaAssembly.GetType("JLio.Extensions.JSchema.RegisterJSchemaPack");
                var registerJSchemaMethod = registerJSchemaPackType?.GetMethod("RegisterJSchema", new[] { typeof(IParseOptions) });
                registerJSchemaMethod?.Invoke(null, new object[] { options });
            }
            catch (Exception)
            {
                // Silently fail if assembly is not available
            }
        });
        return this;
    }

    /// <summary>
    /// Add TimeDate extension functions.
    /// Note: This requires a reference to JLio.Extensions.TimeDate.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithTimeDateExtensions()
    {
        parseConfigurators.Add(options => 
        {
            try 
            {
                var timeDateAssembly = Assembly.Load("JLio.Extensions.TimeDate");
                var registerTimeDatePackType = timeDateAssembly.GetType("JLio.Extensions.TimeDate.RegisterTimeDatePack");
                var registerTimeDateMethod = registerTimeDatePackType?.GetMethod("RegisterTimeDate", new[] { typeof(IParseOptions) });
                registerTimeDateMethod?.Invoke(null, new object[] { options });
            }
            catch (Exception)
            {
                // Silently fail if assembly is not available
            }
        });
        return this;
    }

    /// <summary>
    /// Add all available extensions.
    /// </summary>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder WithAllExtensions()
    {
        return this
            .WithMathExtensions()
            .WithTextExtensions()
            .WithETLExtensions()
            .WithJSchemaExtensions()
            .WithTimeDateExtensions();
    }

    /// <summary>
    /// Add custom configuration for parse options.
    /// </summary>
    /// <param name="configurator">Action to configure parse options</param>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder ConfigureParsing(Action<IParseOptions> configurator)
    {
        parseConfigurators.Add(configurator);
        return this;
    }

    /// <summary>
    /// Add custom configuration for execution context.
    /// </summary>
    /// <param name="configurator">Action to configure execution context</param>
    /// <returns>This builder for method chaining</returns>
    public JLioEngineBuilder ConfigureExecution(Action<IExecutionContext> configurator)
    {
        executionConfigurators.Add(configurator);
        return this;
    }

    /// <summary>
    /// Build the JLio engine with the configured options.
    /// </summary>
    /// <returns>A new JLio engine instance</returns>
    public JLioEngine Build()
    {
        var parseOptions = CreateParseOptions();
        var executionContext = CreateExecutionContext();

        return new JLioEngine(parseOptions, executionContext);
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

        // Use reflection to set the private properties like in CreateDefault
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