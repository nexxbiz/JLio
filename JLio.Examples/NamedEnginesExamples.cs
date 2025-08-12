using System;
using JLio.Client;
using Newtonsoft.Json.Linq;

namespace JLio.Examples;

/// <summary>
/// Examples demonstrating the Named Engines functionality
/// </summary>
public static class NamedEnginesExamples
{
    /// <summary>
    /// Basic example showing how to use pre-registered engines
    /// </summary>
    public static void BasicUsageExample()
    {
        Console.WriteLine("=== Basic Named Engines Usage ===");

        var script = @"[{""path"": ""$.greeting"", ""value"": ""Hello from Named Engine!"", ""command"": ""add""}]";
        var data = new JObject();

        // Use pre-registered engines
        var result1 = JLioNamedEngines.Execute("minimal", script, data.DeepClone());
        var result2 = JLioNamedEngines.Execute("v2", script, data.DeepClone());
        var result3 = JLioNamedEngines.Execute("default", script, data.DeepClone());

        Console.WriteLine($"Minimal Engine Result: {result1.Data}");
        Console.WriteLine($"V2 Engine Result: {result2.Data}");
        Console.WriteLine($"Default Engine Result: {result3.Data}");
    }

    /// <summary>
    /// Example showing how to register custom engines
    /// </summary>
    public static void CustomEngineRegistrationExample()
    {
        Console.WriteLine("\n=== Custom Engine Registration ===");

        // Register a custom engine for order processing
        JLioNamedEngines.Register("order-processor", builder =>
            new JLioEngineBuilder()
                .WithCoreCommands()
                .WithCoreFunctions()
                .WithMathExtensions()
                .WithTextExtensions()
                .Build());

        // Register a minimal engine for health checks
        JLioNamedEngines.Register("health-check", builder =>
            new JLioEngineBuilder()
                .WithCommand<Commands.Add>()
                .WithCommand<Commands.Set>()
                .Build());

        // Use the custom engines
        var orderScript = @"[
            {""path"": ""$.order.processedAt"", ""value"": ""=datetime()"", ""command"": ""add""},
            {""path"": ""$.order.customerName"", ""value"": ""=concat(@.firstName, ' ', @.lastName)"", ""command"": ""add""},
            {""path"": ""$.order.total"", ""value"": ""=calculate(@.quantity * @.price)"", ""command"": ""add""}
        ]";

        var orderData = JToken.Parse(@"{
            ""firstName"": ""John"",
            ""lastName"": ""Doe"",
            ""quantity"": 3,
            ""price"": 25.99
        }");

        var orderResult = JLioNamedEngines.Execute("order-processor", orderScript, orderData);
        Console.WriteLine($"Order Processing Result: {orderResult.Data}");

        // Health check script
        var healthScript = @"[{""path"": ""$.status"", ""value"": ""healthy"", ""command"": ""add""}]";
        var healthResult = JLioNamedEngines.Execute("health-check", healthScript, new JObject());
        Console.WriteLine($"Health Check Result: {healthResult.Data}");
    }

    /// <summary>
    /// Example showing multi-tenant scenario with different engine configurations
    /// </summary>
    public static void MultiTenantExample()
    {
        Console.WriteLine("\n=== Multi-Tenant Scenario ===");

        // Register tenant-specific engines
        JLioNamedEngines.Register("tenant-startup", builder =>
            JLioEngineConfigurations.CreateV1().Build());

        JLioNamedEngines.Register("tenant-business", builder =>
            JLioEngineConfigurations.CreateV2()
                .WithMathExtensions()
                .Build());

        JLioNamedEngines.Register("tenant-enterprise", builder =>
            JLioEngineConfigurations.CreateV3().Build());

        // Process data for different tenants
        var script = @"[
            {""path"": ""$.processed"", ""value"": true, ""command"": ""add""},
            {""path"": ""$.timestamp"", ""value"": ""=datetime()"", ""command"": ""add""}
        ]";
        
        var tenantData = JToken.Parse(@"{""tenant"": ""example"", ""data"": ""sample""}");

        foreach (var tenant in new[] { "startup", "business", "enterprise" })
        {
            var engineName = $"tenant-{tenant}";
            var result = JLioNamedEngines.Execute(engineName, script, tenantData.DeepClone());
            Console.WriteLine($"Tenant {tenant} Result: {result.Data}");
        }
    }

    /// <summary>
    /// Example showing versioned engines for package version management
    /// </summary>
    public static void VersionedEnginesExample()
    {
        Console.WriteLine("\n=== Versioned Engines Example ===");

        // Register versioned engines (conceptual - would use actual version paths in real scenario)
        JLioNamedEngines.RegisterVersioned("math-v1", builder =>
            new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithCoreFunctions()
                .WithMathExtensions("default") // In real scenario: "1.0.0"
                .Build());

        JLioNamedEngines.RegisterVersioned("math-v2", builder =>
            new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithCoreFunctions()
                .WithMathExtensions("default") // In real scenario: "2.0.0"
                .Build());

        var mathScript = @"[{""path"": ""$.calculation"", ""value"": ""=calculate(10 * 5 + 2)"", ""command"": ""add""}]";
        var data = new JObject();

        var v1Result = JLioNamedEngines.ExecuteVersioned("math-v1", mathScript, data.DeepClone());
        var v2Result = JLioNamedEngines.ExecuteVersioned("math-v2", mathScript, data.DeepClone());

        Console.WriteLine($"Math V1 Result: {v1Result.Data}");
        Console.WriteLine($"Math V2 Result: {v2Result.Data}");
    }

    /// <summary>
    /// Example showing factory vs singleton patterns
    /// </summary>
    public static void FactoryVsSingletonExample()
    {
        Console.WriteLine("\n=== Factory vs Singleton Pattern ===");

        // Singleton engine (default behavior)
        JLioNamedEngines.Register("singleton-engine", builder =>
        {
            Console.WriteLine("Creating singleton engine instance");
            return new JLioEngineBuilder().WithCoreCommands().Build();
        }, singleton: true);

        // Factory engine (creates new instance each time)
        JLioNamedEngines.Register("factory-engine", builder =>
        {
            Console.WriteLine("Creating factory engine instance");
            return new JLioEngineBuilder().WithCoreCommands().Build();
        }, singleton: false);

        var script = @"[{""path"": ""$.test"", ""value"": ""instance test"", ""command"": ""add""}]";

        // Get singleton engine multiple times
        Console.WriteLine("Getting singleton engines:");
        var singleton1 = JLioNamedEngines.Get("singleton-engine");
        var singleton2 = JLioNamedEngines.Get("singleton-engine");
        Console.WriteLine($"Same instance: {ReferenceEquals(singleton1, singleton2)}");

        // Get factory engine multiple times
        Console.WriteLine("Getting factory engines:");
        var factory1 = JLioNamedEngines.Get("factory-engine");
        var factory2 = JLioNamedEngines.Get("factory-engine");
        Console.WriteLine($"Same instance: {ReferenceEquals(factory1, factory2)}");
    }

    /// <summary>
    /// Example showing engine management operations
    /// </summary>
    public static void EngineManagementExample()
    {
        Console.WriteLine("\n=== Engine Management Operations ===");

        // Register a temporary engine
        JLioNamedEngines.Register("temporary-engine", builder =>
            new JLioEngineBuilder().WithCoreCommands().Build());

        // List all engines
        Console.WriteLine("Registered Engines:");
        foreach (var name in JLioNamedEngines.GetNames())
        {
            Console.WriteLine($"- {name}");
        }

        // Check if engine exists
        Console.WriteLine($"\nTemporary engine exists: {JLioNamedEngines.HasEngine("temporary-engine")}");

        // Try to get an engine safely
        if (JLioNamedEngines.TryGet("temporary-engine", out var engine))
        {
            Console.WriteLine("Successfully retrieved temporary engine");
        }

        // Remove the temporary engine
        JLioNamedEngines.Remove("temporary-engine");
        Console.WriteLine($"After removal, temporary engine exists: {JLioNamedEngines.HasEngine("temporary-engine")}");
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public static void RunAllExamples()
    {
        Console.WriteLine("JLio Named Engines Examples");
        Console.WriteLine("===========================");

        try
        {
            BasicUsageExample();
            CustomEngineRegistrationExample();
            MultiTenantExample();
            VersionedEnginesExample();
            FactoryVsSingletonExample();
            EngineManagementExample();

            Console.WriteLine("\n=== All Examples Completed Successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError running examples: {ex.Message}");
        }
        finally
        {
            // Clean up
            JLioNamedEngines.Clear();
            Console.WriteLine("\nEngines cleared and reset to defaults.");
        }
    }
}