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