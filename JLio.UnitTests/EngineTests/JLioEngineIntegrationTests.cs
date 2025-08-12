using System;
using JLio.Client;
using JLio.Core.Models;
using JLio.Extensions.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.EngineTests;

[TestFixture]
public class JLioEngineIntegrationTests
{
    [Test]
    public void BackwardCompatibility_ExistingCodeStillWorks()
    {
        // Arrange - This is how users currently use JLio
        var script = @"[
            {""path"": ""$.user.fullName"", ""value"": ""=concat(@.firstName, ' ', @.lastName)"", ""command"": ""add""},
            {""path"": ""$.user.age"", ""value"": 25, ""command"": ""set""}
        ]";
        
        var data = JToken.Parse(@"{
            ""user"": {
                ""firstName"": ""John"",
                ""lastName"": ""Doe"",
                ""age"": 24
            }
        }");

        // Act - Using existing approach (should still work)
        var parseOptions = ParseOptions.CreateDefault().RegisterText();
        var executionContext = ExecutionContext.CreateDefault();
        var parsedScript = JLioConvert.Parse(script, parseOptions);
        var result = parsedScript.Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("John Doe", result.Data.SelectToken("$.user.fullName")?.Value<string>());
        Assert.AreEqual(25, result.Data.SelectToken("$.user.age")?.Value<int>());
    }

    [Test]
    public void NewEngineArchitecture_ProducesSameResults()
    {
        // Arrange - Same script as above
        var script = @"[
            {""path"": ""$.user.fullName"", ""value"": ""=concat(@.firstName, ' ', @.lastName)"", ""command"": ""add""},
            {""path"": ""$.user.age"", ""value"": 25, ""command"": ""set""}
        ]";
        
        var data = JToken.Parse(@"{
            ""user"": {
                ""firstName"": ""John"",
                ""lastName"": ""Doe"",
                ""age"": 24
            }
        }");

        // Act - Using new engine approach
        var engine = JLioEngineConfigurations.CreateV2()
            .WithTextExtensions()
            .Build();
        var result = engine.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("John Doe", result.Data.SelectToken("$.user.fullName")?.Value<string>());
        Assert.AreEqual(25, result.Data.SelectToken("$.user.age")?.Value<int>());
    }

    [Test]
    public void MultipleEngines_CanProcessDifferentScripts()
    {
        // Arrange - Different engines for different requirements
        var basicEngine = JLioEngineConfigurations.CreateV1().Build();
        var mathEngine = JLioEngineConfigurations.CreateV2().WithMathExtensions().Build();
        
        var basicScript = @"[{""path"": ""$.message"", ""value"": ""Hello World"", ""command"": ""add""}]";
        var mathScript = @"[{""path"": ""$.result"", ""value"": ""=calculate(10 * 2 + 5)"", ""command"": ""add""}]";
        
        var data = new JObject();

        // Act
        var basicResult = basicEngine.ParseAndExecute(basicScript, data.DeepClone());
        var mathResult = mathEngine.ParseAndExecute(mathScript, data.DeepClone());

        // Assert
        Assert.IsTrue(basicResult.Success);
        Assert.IsTrue(mathResult.Success);
        Assert.AreEqual("Hello World", basicResult.Data.SelectToken("$.message")?.Value<string>());
        Assert.AreEqual(25, mathResult.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void ConvenienceMethod_ParseAndExecute_Works()
    {
        // Arrange
        var script = @"[{""path"": ""$.greeting"", ""value"": ""Hello from convenience method"", ""command"": ""add""}]";
        var data = new JObject();

        // Act - Using new convenience method
        var result = JLioConvert.ParseAndExecute(script, data);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hello from convenience method", result.Data.SelectToken("$.greeting")?.Value<string>());
    }

    [Test]
    public void ComplexScenario_OrderProcessing_WithMultipleCommands()
    {
        // Arrange - A realistic scenario
        var script = @"[
            {""path"": ""$.order.processedAt"", ""value"": ""=datetime()"", ""command"": ""add""},
            {""path"": ""$.order.totalItems"", ""value"": ""=sum($.order.items[*].quantity)"", ""command"": ""add""},
            {""path"": ""$.order.status"", ""value"": ""processed"", ""command"": ""set""},
            {""path"": ""$.order.customer.displayName"", ""value"": ""=concat(@.firstName, ' ', @.lastName)"", ""command"": ""add""}
        ]";

        var orderData = JToken.Parse(@"{
            ""order"": {
                ""id"": ""ORD-001"",
                ""status"": ""pending"",
                ""items"": [
                    {""productId"": ""P001"", ""quantity"": 2},
                    {""productId"": ""P002"", ""quantity"": 3}
                ],
                ""customer"": {
                    ""firstName"": ""Jane"",
                    ""lastName"": ""Smith""
                }
            }
        }");

        // Act - Using engine with all necessary extensions
        var engine = JLioEngineConfigurations.CreateV2()
            .WithMathExtensions()
            .WithTextExtensions()
            .Build();
        var result = engine.ParseAndExecute(script, orderData);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.order.processedAt"));
        Assert.AreEqual(5, result.Data.SelectToken("$.order.totalItems")?.Value<int>());
        Assert.AreEqual("processed", result.Data.SelectToken("$.order.status")?.Value<string>());
        Assert.AreEqual("Jane Smith", result.Data.SelectToken("$.order.customer.displayName")?.Value<string>());
    }

    [Test]
    public void CustomEngineBuilder_CanCreateSpecializedEngine()
    {
        // Arrange - Build a specialized engine for specific use case
        var specializedEngine = new JLioEngineBuilder()
            .WithCommand<Commands.Add>()
            .WithCommand<Commands.Set>()
            .WithFunction<Functions.Datetime>()
            .WithFunction<Functions.Fetch>()
            .WithMathExtensions()
            .ConfigureParsing(options => {
                // Could add custom functions here
            })
            .Build();

        var script = @"[
            {""path"": ""$.timestamp"", ""value"": ""=datetime()"", ""command"": ""add""},
            {""path"": ""$.calculation"", ""value"": ""=calculate(5 * 4)"", ""command"": ""add""}
        ]";

        // Act
        var result = specializedEngine.ParseAndExecute(script, new JObject());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.timestamp"));
        Assert.AreEqual(20, result.Data.SelectToken("$.calculation")?.Value<int>());
    }
}