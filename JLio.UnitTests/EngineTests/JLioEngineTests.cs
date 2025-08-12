using System;
using JLio.Client;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.ETL;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.EngineTests;

public class JLioEngineTests
{
    [Test]
    public void JLioEngine_CanBeCreated_WithBasicConfiguration()
    {
        // Arrange & Act
        var engine = JLioEngineConfigurations.CreateV1().Build();

        // Assert
        Assert.IsNotNull(engine);
    }

    [Test]
    public void JLioEngine_CanParseAndExecute_SimpleScript()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateV1().Build();
        var script = "[{\"path\":\"$.test\",\"value\":\"hello world\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello world", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngine_CanUse_MathExtensions()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateV2()
            .WithMathExtensions()
            .Build();
        
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate(2 * 3 + 4)\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void JLioEngine_CanUse_TextExtensions()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateV2()
            .WithTextExtensions()
            .Build();
        
        var script = "[{\"path\":\"$.result\",\"value\":\"=concat('Hello', ' ', 'World')\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hello World", result.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void JLioEngine_CanUse_CustomBuilder()
    {
        // Arrange
        var engine = new JLioEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .WithFunction<Extensions.Math.Calculate>()
            .Build();
        
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate(5 + 5)\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void JLioEngine_MultipleEngines_CanCoexist()
    {
        // Arrange
        var basicEngine = JLioEngineConfigurations.CreateV1().Build();
        var advancedEngine = JLioEngineConfigurations.CreateV3().Build();
        
        var basicScript = "[{\"path\":\"$.basic\",\"value\":\"simple\",\"command\":\"add\"}]";
        var advancedScript = "[{\"path\":\"$.advanced\",\"value\":\"=calculate(10 * 2)\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var basicResult = basicEngine.ParseAndExecute(basicScript, data.DeepClone());
        var advancedResult = advancedEngine.ParseAndExecute(advancedScript, data.DeepClone());

        // Assert
        Assert.IsTrue(basicResult.Success);
        Assert.IsTrue(advancedResult.Success);
        Assert.AreEqual("simple", basicResult.Data.SelectToken("$.basic")?.Value<string>());
        Assert.AreEqual(20, advancedResult.Data.SelectToken("$.advanced")?.Value<int>());
    }

    [Test]
    public void JLioEngine_BackwardCompatibility_WithExistingCode()
    {
        // Arrange
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act - Using old approach
        var oldResult = JLioConvert.Parse(script).Execute(data.DeepClone());

        // Act - Using new approach
        var newEngine = JLioEngineConfigurations.CreateLatest().Build();
        var newResult = newEngine.ParseAndExecute(script, data.DeepClone());

        // Assert - Both should produce identical results
        Assert.IsTrue(oldResult.Success);
        Assert.IsTrue(newResult.Success);
        Assert.IsTrue(JToken.DeepEquals(oldResult.Data, newResult.Data));
    }

    [Test]
    public void JLioEngine_ConvenienceMethods_InJLioConvert()
    {
        // Arrange
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = JLioConvert.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngine_CanHandle_ExtensionAssemblyNotAvailable()
    {
        // Arrange - This should not throw even if some extension assemblies are missing
        var engine = new JLioEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .WithMathExtensions() // This might fail silently if assembly not loaded
            .WithTextExtensions() // This might fail silently if assembly not loaded
            .Build();

        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
        {
            var result = engine.ParseAndExecute(script, data);
            Assert.IsTrue(result.Success);
        });
    }

    [Test]
    public void JLioEngine_CustomConfiguration_CanModifyParseOptions()
    {
        // Arrange
        var customFunctionRegistered = false;
        var engine = new JLioEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .ConfigureParsing(options =>
            {
                // Custom configuration
                customFunctionRegistered = true;
            })
            .Build();

        // Act
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var result = engine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(customFunctionRegistered);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void JLioEngine_CustomConfiguration_CanModifyExecutionContext()
    {
        // Arrange
        var customExecutionConfigured = false;
        var engine = new JLioEngineBuilder()
            .WithCoreCommands()
            .WithCoreFunctions()
            .ConfigureExecution(context =>
            {
                // Custom configuration
                customExecutionConfigured = true;
            })
            .Build();

        // Act
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var result = engine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(customExecutionConfigured);
        Assert.IsTrue(result.Success);
    }
}