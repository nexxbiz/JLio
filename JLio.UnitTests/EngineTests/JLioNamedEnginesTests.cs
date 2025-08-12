using System;
using System.Collections.Generic;
using System.Linq;
using JLio.Client;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.EngineTests;

[TestFixture]
public class JLioNamedEnginesTests
{
    [SetUp]
    public void Setup()
    {
        // Clear any previously registered engines and reset to defaults
        JLioNamedEngines.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        JLioNamedEngines.Clear();
    }

    [Test]
    public void DefaultEngines_ArePreRegistered()
    {
        // Arrange & Act
        var engineNames = JLioNamedEngines.GetNames().ToList();

        // Assert
        Assert.Contains("minimal", engineNames);
        Assert.Contains("v1", engineNames);
        Assert.Contains("v2", engineNames);
        Assert.Contains("v3", engineNames);
        Assert.Contains("latest", engineNames);
        Assert.Contains("default", engineNames);
        Assert.Contains("data-transformation", engineNames);
        Assert.Contains("etl", engineNames);
    }

    [Test]
    public void CanExecute_WithDefaultEngines()
    {
        // Arrange
        var script = @"[{""path"": ""$.result"", ""value"": ""Hello World"", ""command"": ""add""}]";
        var data = new JObject();

        // Act
        var minimalResult = JLioNamedEngines.Execute("minimal", script, data);
        var v2Result = JLioNamedEngines.Execute("v2", script, data);
        var defaultResult = JLioNamedEngines.Execute("default", script, data);

        // Assert
        Assert.IsTrue(minimalResult.Success);
        Assert.IsTrue(v2Result.Success);
        Assert.IsTrue(defaultResult.Success);
        
        Assert.AreEqual("Hello World", minimalResult.Data.SelectToken("$.result")?.Value<string>());
        Assert.AreEqual("Hello World", v2Result.Data.SelectToken("$.result")?.Value<string>());
        Assert.AreEqual("Hello World", defaultResult.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void CanRegister_CustomEngine()
    {
        // Arrange
        var customEngineName = "my-custom-engine";
        
        // Act
        JLioNamedEngines.Register(customEngineName, builder =>
            builder.WithCoreCommands()
                   .WithCoreFunctions()
                   .Build());

        // Assert
        Assert.IsTrue(JLioNamedEngines.HasEngine(customEngineName));
        
        var engine = JLioNamedEngines.Get(customEngineName);
        Assert.IsNotNull(engine);
    }

    [Test]
    public void CustomEngine_CanExecuteScripts()
    {
        // Arrange
        JLioNamedEngines.Register("test-engine", builder =>
            new JLioEngineBuilder()
                .WithCoreCommands()
                .WithCoreFunctions()
                .Build());

        var script = @"[{""path"": ""$.timestamp"", ""value"": ""=datetime()"", ""command"": ""add""}]";
        var data = new JObject();

        // Act
        var result = JLioNamedEngines.Execute("test-engine", script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.timestamp"));
    }

    [Test]
    public void CanRegister_FactoryBasedEngine()
    {
        // Arrange
        var factoryCallCount = 0;
        
        JLioNamedEngines.Register("factory-engine", builder =>
        {
            factoryCallCount++;
            return new JLioEngineBuilder()
                .WithCoreCommands()
                .Build();
        }, singleton: false); // Factory mode

        // Act
        var engine1 = JLioNamedEngines.Get("factory-engine");
        var engine2 = JLioNamedEngines.Get("factory-engine");

        // Assert
        Assert.AreEqual(2, factoryCallCount); // Should be called twice for factory mode
        Assert.AreNotSame(engine1, engine2); // Different instances
    }

    [Test]
    public void CanRegister_SingletonEngine()
    {
        // Arrange
        var singletonCallCount = 0;
        
        JLioNamedEngines.Register("singleton-engine", builder =>
        {
            singletonCallCount++;
            return new JLioEngineBuilder()
                .WithCoreCommands()
                .Build();
        }, singleton: true); // Singleton mode (default)

        // Act
        var engine1 = JLioNamedEngines.Get("singleton-engine");
        var engine2 = JLioNamedEngines.Get("singleton-engine");

        // Assert
        Assert.AreEqual(1, singletonCallCount); // Should be called only once for singleton mode
        Assert.AreSame(engine1, engine2); // Same instance
    }

    [Test]
    public void TryGet_ReturnsCorrectly()
    {
        // Arrange
        JLioNamedEngines.Register("existing-engine", builder =>
            new JLioEngineBuilder().WithCoreCommands().Build());

        // Act & Assert
        Assert.IsTrue(JLioNamedEngines.TryGet("existing-engine", out var existingEngine));
        Assert.IsNotNull(existingEngine);

        Assert.IsFalse(JLioNamedEngines.TryGet("non-existing-engine", out var nonExistingEngine));
        Assert.IsNull(nonExistingEngine);
    }

    [Test]
    public void Get_ThrowsException_WhenEngineNotFound()
    {
        // Act & Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => 
            JLioNamedEngines.Get("non-existent-engine"));
        
        Assert.That(ex.Message, Contains.Substring("No engine registered with name 'non-existent-engine'"));
    }

    [Test]
    public void CanRemove_RegisteredEngine()
    {
        // Arrange
        JLioNamedEngines.Register("removable-engine", builder =>
            new JLioEngineBuilder().WithCoreCommands().Build());

        Assert.IsTrue(JLioNamedEngines.HasEngine("removable-engine"));

        // Act
        var removed = JLioNamedEngines.Remove("removable-engine");

        // Assert
        Assert.IsTrue(removed);
        Assert.IsFalse(JLioNamedEngines.HasEngine("removable-engine"));
    }

    [Test]
    public void CanRegister_VersionedEngine()
    {
        // Arrange
        var versionedEngineName = "versioned-test";

        // Act
        JLioNamedEngines.RegisterVersioned(versionedEngineName, builder =>
            builder.WithCoreCommands()
                   .WithCoreFunctions()
                   .Build());

        // Assert
        Assert.IsTrue(JLioNamedEngines.HasVersionedEngine(versionedEngineName));
        
        using var engine = JLioNamedEngines.GetVersioned(versionedEngineName);
        Assert.IsNotNull(engine);
    }

    [Test]
    public void VersionedEngine_CanExecuteScripts()
    {
        // Arrange
        JLioNamedEngines.RegisterVersioned("versioned-executor", builder =>
            new JLioVersionedEngineBuilder()
                .WithCoreCommands()
                .WithCoreFunctions()
                .Build());

        var script = @"[{""path"": ""$.test"", ""value"": ""versioned success"", ""command"": ""add""}]";
        var data = new JObject();

        // Act
        var result = JLioNamedEngines.ExecuteVersioned("versioned-executor", script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("versioned success", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void MultipleEngines_CanCoexist()
    {
        // Arrange
        JLioNamedEngines.Register("basic-engine", builder =>
            new JLioEngineBuilder()
                .WithCommand<Commands.Add>()
                .WithCommand<Commands.Set>()
                .Build());

        JLioNamedEngines.Register("advanced-engine", builder =>
            JLioEngineConfigurations.CreateV2().Build());

        var basicScript = @"[{""path"": ""$.basic"", ""value"": ""basic result"", ""command"": ""add""}]";
        var advancedScript = @"[{""path"": ""$.advanced"", ""value"": ""advanced result"", ""command"": ""add""}]";
        var data = new JObject();

        // Act
        var basicResult = JLioNamedEngines.Execute("basic-engine", basicScript, data.DeepClone());
        var advancedResult = JLioNamedEngines.Execute("advanced-engine", advancedScript, data.DeepClone());

        // Assert
        Assert.IsTrue(basicResult.Success);
        Assert.IsTrue(advancedResult.Success);
        Assert.AreEqual("basic result", basicResult.Data.SelectToken("$.basic")?.Value<string>());
        Assert.AreEqual("advanced result", advancedResult.Data.SelectToken("$.advanced")?.Value<string>());
    }

    [Test]
    public void Clear_ResetsToDefaults()
    {
        // Arrange
        JLioNamedEngines.Register("custom-temp-engine", builder =>
            new JLioEngineBuilder().WithCoreCommands().Build());
        
        Assert.IsTrue(JLioNamedEngines.HasEngine("custom-temp-engine"));

        // Act
        JLioNamedEngines.Clear();

        // Assert
        Assert.IsFalse(JLioNamedEngines.HasEngine("custom-temp-engine"));
        
        // But defaults should be back
        Assert.IsTrue(JLioNamedEngines.HasEngine("default"));
        Assert.IsTrue(JLioNamedEngines.HasEngine("v2"));
        Assert.IsTrue(JLioNamedEngines.HasEngine("minimal"));
    }

    [Test]
    public void InvalidEngineName_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            JLioNamedEngines.Register("", builder => new JLioEngineBuilder().Build()));
        
        Assert.Throws<ArgumentException>(() => 
            JLioNamedEngines.Register(null, builder => new JLioEngineBuilder().Build()));
        
        Assert.Throws<ArgumentException>(() => 
            JLioNamedEngines.Register("   ", builder => new JLioEngineBuilder().Build()));
    }

    [Test]
    public void MultiTenantScenario_WorksCorrectly()
    {
        // Arrange - Simulating multi-tenant engine setup
        JLioNamedEngines.Register("tenant-basic", builder =>
            JLioEngineConfigurations.CreateV1().Build());

        JLioNamedEngines.Register("tenant-premium", builder =>
            JLioEngineConfigurations.CreateV2()
                .WithMathExtensions()
                .Build());

        JLioNamedEngines.Register("tenant-enterprise", builder =>
            JLioEngineConfigurations.CreateV3().Build());

        var script = @"[{""path"": ""$.tenant"", ""value"": ""processed"", ""command"": ""add""}]";
        var data = new JObject();

        // Act
        var basicResult = JLioNamedEngines.Execute("tenant-basic", script, data.DeepClone());
        var premiumResult = JLioNamedEngines.Execute("tenant-premium", script, data.DeepClone());
        var enterpriseResult = JLioNamedEngines.Execute("tenant-enterprise", script, data.DeepClone());

        // Assert
        Assert.IsTrue(basicResult.Success);
        Assert.IsTrue(premiumResult.Success);
        Assert.IsTrue(enterpriseResult.Success);
        
        Assert.AreEqual("processed", basicResult.Data.SelectToken("$.tenant")?.Value<string>());
        Assert.AreEqual("processed", premiumResult.Data.SelectToken("$.tenant")?.Value<string>());
        Assert.AreEqual("processed", enterpriseResult.Data.SelectToken("$.tenant")?.Value<string>());
    }

    [Test]
    public void GetEngineNames_ReturnsAllRegisteredNames()
    {
        // Arrange
        JLioNamedEngines.Register("custom1", builder => new JLioEngineBuilder().Build());
        JLioNamedEngines.Register("custom2", builder => new JLioEngineBuilder().Build());

        // Act
        var names = JLioNamedEngines.GetNames().ToList();

        // Assert
        Assert.Contains("custom1", names);
        Assert.Contains("custom2", names);
        Assert.Contains("default", names); // Default engines should still be there
        Assert.Contains("v1", names);
    }

    [Test]
    public void VersionedEngineNames_AreTrackedSeparately()
    {
        // Arrange
        JLioNamedEngines.RegisterVersioned("versioned1", builder => new JLioVersionedEngineBuilder().Build());
        JLioNamedEngines.RegisterVersioned("versioned2", builder => new JLioVersionedEngineBuilder().Build());

        // Act
        var standardNames = JLioNamedEngines.GetNames().ToList();
        var versionedNames = JLioNamedEngines.GetVersionedNames().ToList();

        // Assert
        Assert.Contains("versioned1", versionedNames);
        Assert.Contains("versioned2", versionedNames);
        
        // Versioned engines should not appear in standard names
        Assert.That(standardNames, Does.Not.Contain("versioned1"));
        Assert.That(standardNames, Does.Not.Contain("versioned2"));
    }
}