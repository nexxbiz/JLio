using System.Linq;
using JLio.Commands;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class PropertyFieldBackwardsCompatibilityTests
{
    private IExecutionContext executionContext;

    [SetUp]
    public void Setup()
    {
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void Add_LegacySyntax_ShouldWorkExactlyAsbefore()
    {
        // Test that existing legacy syntax continues to work unchanged
        var data = JToken.Parse(@"{
            ""customers"": [
                {""id"": ""C001"", ""name"": ""Alice""},
                {""id"": ""C002"", ""name"": ""Bob""}
            ]
        }");

        // Legacy syntax - property name embedded in path
        var addCommand = new Add("$.customers[*].status", new JValue("active"));
        var result = addCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("active", result.Data.SelectToken("$.customers[0].status")?.Value<string>());
        Assert.AreEqual("active", result.Data.SelectToken("$.customers[1].status")?.Value<string>());
    }

    [Test]
    public void Add_NewSyntax_ShouldProvideClearerSemantics()
    {
        // Test the new property field syntax
        var data = JToken.Parse(@"{
            ""customers"": [
                {""id"": ""C001"", ""name"": ""Alice""},
                {""id"": ""C002"", ""name"": ""Bob""}
            ]
        }");

        // New syntax - path points to objects, property specifies what to add
        var addCommand = new Add();
        addCommand.Path = "$.customers[*]";
        addCommand.Property = "status";
        addCommand.Value = new FunctionSupportedValue(new FixedValue(new JValue("active")));

        var result = addCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("active", result.Data.SelectToken("$.customers[0].status")?.Value<string>());
        Assert.AreEqual("active", result.Data.SelectToken("$.customers[1].status")?.Value<string>());
    }

    [Test]
    public void Add_NewSyntaxWithConstructor_ShouldWork()
    {
        var data = JToken.Parse(@"{""items"": [{""id"": 1}, {""id"": 2}]}");

        // Using new constructor
        var addCommand = new Add("$.items[*]", "active", new JValue(true));
        var result = addCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(true, result.Data.SelectToken("$.items[0].active")?.Value<bool>());
        Assert.AreEqual(true, result.Data.SelectToken("$.items[1].active")?.Value<bool>());
    }

    [Test]
    public void Add_ArrayOperation_LegacySyntax_ShouldWork()
    {
        var data = JToken.Parse(@"{""tags"": [""tag1"", ""tag2""]}");

        // Legacy array syntax
        var addCommand = new Add("$.tags", new JValue("tag3"));
        var result = addCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("tag3", result.Data.SelectToken("$.tags[2]")?.Value<string>());
    }

    [Test]
    public void Add_ArrayOperation_NewSyntax_ShouldWork()
    {
        var data = JToken.Parse(@"{""tags"": [""tag1"", ""tag2""]}");

        // New array syntax - path points to array, no property field needed
        var addCommand = new Add();
        addCommand.Path = "$.tags";
        addCommand.Value = new FunctionSupportedValue(new FixedValue(new JValue("tag3")));

        var result = addCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("tag3", result.Data.SelectToken("$.tags[2]")?.Value<string>());
    }

    [Test]
    public void Set_NewSyntax_ShouldWork()
    {
        var data = JToken.Parse(@"{""users"": [{""name"": ""Alice"", ""status"": ""active""}]}");

        // New syntax for Set command
        var setCommand = new Set("$.users[*]", "status", new JValue("inactive"));
        var result = setCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("inactive", result.Data.SelectToken("$.users[0].status")?.Value<string>());
    }

    [Test]
    public void Put_NewSyntax_ShouldWork()
    {
        var data = JToken.Parse(@"{""products"": [{""name"": ""Product A""}]}");

        // New syntax for Put command - creates new property
        var putCommand = new Put("$.products[*]", "price", new JValue(99.99));
        var result = putCommand.Execute(data, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(99.99, result.Data.SelectToken("$.products[0].price")?.Value<decimal>());
    }

    [Test]
    public void PropertyFieldValidation_ShouldPreventInvalidPropertyNames()
    {
        var addCommand = new Add();
        addCommand.Path = "$.items[*]";
        addCommand.Property = "invalid.property.name"; // Invalid - contains dots
        addCommand.Value = new FunctionSupportedValue(new FixedValue(new JValue("test")));

        var validation = addCommand.ValidateCommandInstance();
        
        Assert.IsFalse(validation.IsValid);
        Assert.IsTrue(validation.ValidationMessages.Any(m => m.Contains("simple property name")));
    }

    [Test]
    public void BothSyntaxes_ShouldProduceSameResult()
    {
        // Verify that both syntaxes produce identical results
        var data1 = JToken.Parse(@"{""items"": [{""id"": 1}, {""id"": 2}]}");
        var data2 = data1.DeepClone();

        // Legacy syntax
        var legacyCommand = new Add("$.items[*].active", new JValue(true));
        var legacyResult = legacyCommand.Execute(data1, executionContext);

        // New syntax  
        var newCommand = new Add("$.items[*]", "active", new JValue(true));
        var newResult = newCommand.Execute(data2, executionContext);

        // Results should be identical
        Assert.IsTrue(JToken.DeepEquals(legacyResult.Data, newResult.Data));
    }
}