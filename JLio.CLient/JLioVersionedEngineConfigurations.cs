namespace JLio.Client;

/// <summary>
/// Predefined JLio versioned engine configurations that support multiple package versions.
/// Provides easy access to commonly used engine setups with version isolation capabilities.
/// </summary>
public static class JLioVersionedEngineConfigurations
{
    /// <summary>
    /// Create a versioned engine builder for multi-tenant scenarios where different tenants
    /// may require different versions of the same extension packages.
    /// </summary>
    /// <returns>Versioned engine builder configured for multi-tenant use</returns>
    public static JLioVersionedEngineBuilder CreateMultiTenant()
    {
        return new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions();
    }

    /// <summary>
    /// Create a versioned engine builder for development/testing scenarios where you need
    /// to test against multiple versions of extension packages.
    /// </summary>
    /// <returns>Versioned engine builder configured for development testing</returns>
    public static JLioVersionedEngineBuilder CreateForTesting()
    {
        return new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithAdvancedCommands()
            .WithCoreFunctions();
    }

    /// <summary>
    /// Create a versioned engine builder for migration scenarios where you need to
    /// gradually migrate from one version of extensions to another.
    /// </summary>
    /// <returns>Versioned engine builder configured for migration scenarios</returns>
    public static JLioVersionedEngineBuilder CreateForMigration()
    {
        return new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithAdvancedCommands()
            .WithCoreFunctions();
    }

    /// <summary>
    /// Create a versioned engine builder with specific Math extension versions.
    /// Useful when you need to maintain compatibility with older Math package versions.
    /// </summary>
    /// <param name="mathVersion">Version of Math extensions to load</param>
    /// <param name="mathAssemblyPath">Optional custom path to the Math assembly</param>
    /// <returns>Versioned engine builder with specific Math version</returns>
    public static JLioVersionedEngineBuilder CreateWithMathVersion(string mathVersion, string mathAssemblyPath = null)
    {
        return new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .WithMathExtensions(mathVersion, mathAssemblyPath);
    }

    /// <summary>
    /// Create a versioned engine builder with specific Text extension versions.
    /// Useful when you need to maintain compatibility with older Text package versions.
    /// </summary>
    /// <param name="textVersion">Version of Text extensions to load</param>
    /// <param name="textAssemblyPath">Optional custom path to the Text assembly</param>
    /// <returns>Versioned engine builder with specific Text version</returns>
    public static JLioVersionedEngineBuilder CreateWithTextVersion(string textVersion, string textAssemblyPath = null)
    {
        return new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .WithTextExtensions(textVersion, textAssemblyPath);
    }

    /// <summary>
    /// Create a versioned engine builder with specific versions of multiple extensions.
    /// This is the most flexible approach for complex version requirements.
    /// </summary>
    /// <param name="mathVersion">Version of Math extensions</param>
    /// <param name="textVersion">Version of Text extensions</param>
    /// <param name="etlVersion">Version of ETL extensions</param>
    /// <returns>Versioned engine builder with multiple specific versions</returns>
    public static JLioVersionedEngineBuilder CreateWithSpecificVersions(
        string mathVersion = "default", 
        string textVersion = "default", 
        string etlVersion = "default")
    {
        var builder = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithAdvancedCommands()
            .WithCoreFunctions();

        if (mathVersion != null)
            builder.WithMathExtensions(mathVersion);
        
        if (textVersion != null)
            builder.WithTextExtensions(textVersion);
        
        if (etlVersion != null)
            builder.WithETLExtensions(etlVersion);

        return builder;
    }
}