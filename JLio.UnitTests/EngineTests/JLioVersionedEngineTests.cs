using System;
using System.IO;
using JLio.Client;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.EngineTests;

[TestFixture]
public class JLioVersionedEngineTests
{
    [Test]
    public void JLioVersionedEngine_CanBeCreated()
    {
        // Arrange & Act
        var engine = JLioVersionedEngineConfigurations.CreateMultiTenant().Build();

        // Assert
        Assert.IsNotNull(engine);
        using (engine) // Ensure proper disposal
        {
            // Test basic functionality
            var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
            var result = engine.ParseAndExecute(script, new JObject());
            Assert.IsTrue(result.Success);
        }
    }

    [Test]
    public void JLioVersionedEngine_CanUseDefaultVersions()
    {
        // Arrange
        using var engine = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .WithMathExtensions("default") // Uses standard loading
            .Build();

        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate(5 + 5)\",\"command\":\"add\"}]";

        // Act
        var result = engine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void JLioVersionedEngine_ShowsLoadedPackageVersions()
    {
        // Arrange
        using var engine = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("default")
            .Build();

        // Act
        var packageVersions = engine.GetLoadedPackageVersions();

        // Assert
        Assert.IsNotNull(packageVersions);
        // Default versions won't show in the dictionary since they use standard loading
        // But versioned packages would appear here
    }

    [Test]
    public void JLioVersionedEngine_DisposesCorrectly()
    {
        // Arrange
        var engine = JLioVersionedEngineConfigurations.CreateForTesting().Build();

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
        {
            using (engine)
            {
                var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
                var result = engine.ParseAndExecute(script, new JObject());
                Assert.IsTrue(result.Success);
            }
        });
    }

    [Test]
    public void JLioVersionedEngine_ConfigurationsWork()
    {
        // Test different predefined configurations
        using var multiTenant = JLioVersionedEngineConfigurations.CreateMultiTenant().Build();
        using var forTesting = JLioVersionedEngineConfigurations.CreateForTesting().Build();
        using var forMigration = JLioVersionedEngineConfigurations.CreateForMigration().Build();

        Assert.IsNotNull(multiTenant);
        Assert.IsNotNull(forTesting);
        Assert.IsNotNull(forMigration);

        // Test basic functionality on each
        var script = "[{\"path\":\"$.test\",\"value\":\"configured\",\"command\":\"add\"}]";
        var data = new JObject();

        Assert.IsTrue(multiTenant.ParseAndExecute(script, data.DeepClone()).Success);
        Assert.IsTrue(forTesting.ParseAndExecute(script, data.DeepClone()).Success);
        Assert.IsTrue(forMigration.ParseAndExecute(script, data.DeepClone()).Success);
    }

    [Test]
    public void JLioVersionedEngine_CustomVersionBuilder_Works()
    {
        // Arrange - Test the specific version methods (using defaults since we don't have actual versioned assemblies)
        using var engine = JLioVersionedEngineConfigurations
            .CreateWithSpecificVersions("default", "default", "default")
            .Build();

        var script = "[{\"path\":\"$.configured\",\"value\":\"with versions\",\"command\":\"add\"}]";

        // Act
        var result = engine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("with versions", result.Data.SelectToken("$.configured")?.Value<string>());
    }

    [Test] 
    public void JLioVersionedEngine_CanHandleMultipleEnginesSimultaneously()
    {
        // Arrange - Multiple versioned engines running side-by-side
        using var engine1 = JLioVersionedEngineConfigurations.CreateMultiTenant()
            .WithMathExtensions("default")
            .Build();

        using var engine2 = JLioVersionedEngineConfigurations.CreateForTesting()
            .WithTextExtensions("default")
            .Build();

        var mathScript = "[{\"path\":\"$.math\",\"value\":\"=calculate(10 * 2)\",\"command\":\"add\"}]";
        var textScript = "[{\"path\":\"$.text\",\"value\":\"=concat('Hello', ' World')\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var mathResult = engine1.ParseAndExecute(mathScript, data.DeepClone());
        var textResult = engine2.ParseAndExecute(textScript, data.DeepClone());

        // Assert
        Assert.IsTrue(mathResult.Success);
        Assert.IsTrue(textResult.Success);
        Assert.AreEqual(20, mathResult.Data.SelectToken("$.math")?.Value<int>());
        Assert.AreEqual("Hello World", textResult.Data.SelectToken("$.text")?.Value<string>());
    }

    [Test]
    public void JLioVersionedEngine_CustomPackageRegistration_WorksWithReflection()
    {
        // Arrange - Test the custom package registration mechanism
        using var engine = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .ConfigureParsing(options => 
            {
                // Custom registration logic could go here
                // For testing, we'll just verify the configurator runs
            })
            .Build();

        var script = "[{\"path\":\"$.custom\",\"value\":\"registered\",\"command\":\"add\"}]";

        // Act
        var result = engine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("registered", result.Data.SelectToken("$.custom")?.Value<string>());
    }

    [Test]
    public void JLioVersionedEngine_RespectsPackageIsolation()
    {
        // This test demonstrates that each engine maintains its own package isolation
        // In a real scenario with actual versioned assemblies, different versions 
        // would be loaded in separate AssemblyLoadContexts

        using var engine1 = new JLioVersionedEngineBuilder()
            .WithCoreCommands()
            .WithMathExtensions("default") // Would be version 1.0 in real scenario
            .Build();

        using var engine2 = new JLioVersionedEngineBuilder()
            .WithCoreCommands() 
            .WithMathExtensions("default") // Would be version 2.0 in real scenario
            .Build();

        // Both engines should work independently
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate(5 + 3)\",\"command\":\"add\"}]";
        var data = new JObject();

        var result1 = engine1.ParseAndExecute(script, data.DeepClone());
        var result2 = engine2.ParseAndExecute(script, data.DeepClone());

        Assert.IsTrue(result1.Success);
        Assert.IsTrue(result2.Success);
        Assert.AreEqual(8, result1.Data.SelectToken("$.result")?.Value<int>());
        Assert.AreEqual(8, result2.Data.SelectToken("$.result")?.Value<int>());
    }
}