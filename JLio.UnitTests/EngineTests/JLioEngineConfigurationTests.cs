using System;
using System.Collections.Generic;
using JLio.Client;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.EngineTests;

public class JLioEngineConfigurationTests
{
    [Test]
    public void JLioEngineConfigurations_CreateV1_HasBasicFeatures()
    {
        // Arrange & Act
        var engine = JLioEngineConfigurations.CreateV1().Build();
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngineConfigurations_CreateV2_HasAdvancedCommands()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateV2().Build();
        var script = @"[{
            ""firstPath"": ""$.first"",
            ""secondPath"": ""$.second"",
            ""command"": ""compare"",
            ""resultPath"": ""$.result"", 
            ""settings"" : {}
        }]";
        var data = JToken.Parse(@"{""first"": ""same"", ""second"": ""same"", ""result"" : {} }");

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.Data.SelectToken("$.result[0].foundDifference")?.Value<bool>());

    }

    [Test]
    public void JLioEngineConfigurations_CreateV3_HasAllExtensions()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateV3().Build();

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => {
            var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
            var result = engine.ParseAndExecute(script, new JObject());
            Assert.IsTrue(result.Success);
        });
    }

    [Test]
    public void JLioEngineConfigurations_CreateMinimal_HasOnlyBasicFeatures()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateMinimal().Build();
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngineConfigurations_CreateForDataTransformation_Works()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateForDataTransformation().Build();
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngineConfigurations_CreateForETL_Works()
    {
        // Arrange
        var engine = JLioEngineConfigurations.CreateForETL().Build();
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Data.SelectToken("$.test")?.Value<string>());
    }

    [Test]
    public void JLioEngineConfigurations_CreateLatest_IsSameAsV3()
    {
        // Arrange
        var latestEngine = JLioEngineConfigurations.CreateLatest().Build();
        var v3Engine = JLioEngineConfigurations.CreateV3().Build();
        
        var script = "[{\"path\":\"$.test\",\"value\":\"hello\",\"command\":\"add\"}]";
        var data = new JObject();

        // Act
        var latestResult = latestEngine.ParseAndExecute(script, data.DeepClone());
        var v3Result = v3Engine.ParseAndExecute(script, data.DeepClone());

        // Assert
        Assert.IsTrue(latestResult.Success);
        Assert.IsTrue(v3Result.Success);
        Assert.IsTrue(JToken.DeepEquals(latestResult.Data, v3Result.Data));
    }

    [Test]
    public void JLioEngineBuilder_CanChain_AllMethods()
    {
        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => {
            var engine = new JLioEngineBuilder()
                .WithCoreCommands()
                .WithAdvancedCommands()
                .WithCoreFunctions()
                .WithMathExtensions()
                .WithTextExtensions()
                .WithETLExtensions()
                .WithJSchemaExtensions()
                .WithTimeDateExtensions()
                .ConfigureParsing(options => { })
                .ConfigureExecution(context => { })
                .Build();

            Assert.IsNotNull(engine);
        });
    }
}