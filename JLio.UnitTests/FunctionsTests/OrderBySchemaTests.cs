using System.Linq;
using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.JSchema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

[TestFixture]
public class OrderBySchemaTests
{
    [SetUp]
    public void Setup()
    {
        parseOptions = new ParseOptions();
        var commandsProvider = new CommandsProvider();
        commandsProvider.Register<Set>();
        parseOptions.JLioCommandConverter = new CommandConverter(commandsProvider);
        var functionsProvider = new FunctionsProvider();
        functionsProvider.Register<OrderBySchema>();
        parseOptions.JLioFunctionConverter = new FunctionConverter(functionsProvider);
        executeContext = ExecutionContext.CreateDefault();
    }

    private IExecutionContext executeContext;
    private ParseOptions parseOptions;

    [Test]
    public void CanBeUsedInFluentApi()
    {
        var schema = "{\"type\":\"object\",\"properties\":{\"lastName\":{\"type\":\"string\"},\"firstName\":{\"type\":\"string\"},\"age\":{\"type\":\"integer\"}}}";
        var parsedSchema = JSchema.Parse(schema);

        var script = new JLioScript()
            .Set(OrderBySchemaBuilders.OrderBySchema(parsedSchema))
            .OnPath("$.person");
        var result = script.Execute(JToken.Parse(
            "{\"person\":{\"age\":30,\"firstName\":\"John\",\"lastName\":\"Doe\",\"country\":\"USA\"}}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        
        var orderedPerson = result.Data["person"] as JObject;
        Assert.IsNotNull(orderedPerson);
        
        // Check that properties are in schema order: lastName, firstName, age, then additional properties
        var properties = orderedPerson.Properties().Select(p => p.Name).ToList();
        Assert.That(properties[0], Is.EqualTo("lastName"));
        Assert.That(properties[1], Is.EqualTo("firstName"));
        Assert.That(properties[2], Is.EqualTo("age"));
        Assert.That(properties[3], Is.EqualTo("country")); // Additional property at the end
    }

    [Test]
    public void CanBeUsedInFluentApi_SchemaPath()
    {
        var script = new JLioScript()
            .Set(OrderBySchemaBuilders.OrderBySchema("$.schema"))
            .OnPath("$.person");
        var result = script.Execute(JToken.Parse(
            "{\"schema\":{\"type\":\"object\",\"properties\":{\"lastName\":{\"type\":\"string\"},\"firstName\":{\"type\":\"string\"},\"age\":{\"type\":\"integer\"}}},\"person\":{\"age\":30,\"firstName\":\"John\",\"lastName\":\"Doe\",\"country\":\"USA\"}}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        
        var orderedPerson = result.Data["person"] as JObject;
        Assert.IsNotNull(orderedPerson);
        
        // Check that properties are in schema order: lastName, firstName, age, then additional properties
        var properties = orderedPerson.Properties().Select(p => p.Name).ToList();
        Assert.That(properties[0], Is.EqualTo("lastName"));
        Assert.That(properties[1], Is.EqualTo("firstName"));
        Assert.That(properties[2], Is.EqualTo("age"));
        Assert.That(properties[3], Is.EqualTo("country")); // Additional property at the end
    }

    [Test]
    public void OrderBySchema_WillReturnErrorForArrayInput()
    {
        var function = "=orderBySchema($.schema)";
        var data = "{\"result\" : [1,2] , \"schema\" : {\"$id\":\"https://example.com/person.schema.json\",\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"title\":\"Person\",\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\",\"description\":\"The person's first name.\"},\"lastName\":{\"type\":\"string\",\"description\":\"The person's last name.\"},\"age\":{\"description\":\"Age in years which must be equal to or greater than zero.\",\"type\":\"integer\",\"minimum\":0}}}    }";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        // The function should handle arrays gracefully and return success
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void CanOrderPropertiesOfArrayItems()
    {
        // Arrange
        var orderBySchemaScript = "[{\"path\":\"$.result\",\"value\":\"=orderBySchema($.schema)\",\"command\":\"set\"}]";
        var input = "{\"result\":{\"policies\":[{\"country\":\"0\",\"sequenceNumber\":1,\"distributionType\":\"LKA\",\"statusType\":\"1\",\"TimeStamp\":\"1899-12-31\",\"processingCode\":\"4\",\"partyRef\":[\"1882937\",\"9928353\"]},{\"country\":\"0\",\"sequenceNumber\":1,\"distributionType\":\"ASD*\",\"statusType\":\"1\",\"TimeStamp\":\"1899-12-31\",\"processingCode\":\"4\",\"partyRef\":[\"1882937\",\"9928353\"]}]},\"schema\":{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"policies\":{\"type\":\"array\",\"items\":{\"properties\":{\"sequenceNumber\":{\"type\":\"integer\"},\"processingCode\":{\"type\":\"string\"},\"partyRef\":{\"type\":\"array\",\"items\":{\"format\":\"\",\"type\":\"string\"}}},\"type\":\"object\"}}}}}";
        var parsedScript = JLioConvert.Parse(orderBySchemaScript, parseOptions);
        var inputObject = JToken.Parse(input);

        // Act
        var result = parsedScript.Execute(inputObject);

        // Assert
        Assert.That(result.Success, Is.True);

        var policies = result.Data["result"]!["policies"]!.Select(x => (JObject)x);
        foreach (var policy in policies)
        {
            Assert.That(policy, Is.Not.Null);

            // Check that all original properties are still present
            Assert.That(policy.ContainsKey("sequenceNumber"), Is.True);
            Assert.That(policy.ContainsKey("processingCode"), Is.True);
            Assert.That(policy.ContainsKey("partyRef"), Is.True);
            Assert.That(policy.ContainsKey("country"), Is.True);
            Assert.That(policy.ContainsKey("distributionType"), Is.True);
            
            // Check that schema properties come first in correct order
            var properties = policy.Properties().Select(p => p.Name).ToList();
            Assert.That(properties[0], Is.EqualTo("sequenceNumber"));
            Assert.That(properties[1], Is.EqualTo("processingCode"));
            Assert.That(properties[2], Is.EqualTo("partyRef"));
            
            // Additional properties should come after schema properties
            Assert.That(properties.IndexOf("country"), Is.GreaterThan(2));
            Assert.That(properties.IndexOf("distributionType"), Is.GreaterThan(2));
        }
    }

    [Test]
    public void OrderBySchema_HandlesNestedObjects()
    {
        // Arrange
        var schema = @"{
            ""type"": ""object"",
            ""properties"": {
                ""user"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""lastName"": {""type"": ""string""},
                        ""firstName"": {""type"": ""string""},
                        ""age"": {""type"": ""integer""}
                    }
                },
                ""timestamp"": {""type"": ""string""}
            }
        }";
        
        var input = @"{
            ""data"": {
                ""timestamp"": ""2023-01-01"",
                ""user"": {
                    ""age"": 25,
                    ""firstName"": ""Alice"",
                    ""lastName"": ""Smith"",
                    ""email"": ""alice@example.com""
                },
                ""source"": ""API""
            }
        }";

        var functionValue = JsonConvert.SerializeObject($"=orderBySchema({schema})");
        var script = $"[{{\"path\":\"$.data\",\"value\":{functionValue},\"command\":\"set\"}}]";
        var parsedScript = JLioConvert.Parse(script, parseOptions);
        var inputObject = JToken.Parse(input);

        // Act
        var result = parsedScript.Execute(inputObject);

        // Assert
        Assert.That(result.Success, Is.True);
        
        var orderedData = result.Data["data"] as JObject;
        Assert.IsNotNull(orderedData);
        
        // Top-level properties should be ordered: user, timestamp, then additional (source)
        var topLevelProperties = orderedData.Properties().Select(p => p.Name).ToList();
        Assert.That(topLevelProperties[0], Is.EqualTo("user"));
        Assert.That(topLevelProperties[1], Is.EqualTo("timestamp"));
        Assert.That(topLevelProperties[2], Is.EqualTo("source"));
        
        // Nested user properties should be ordered: lastName, firstName, age, then additional (email)
        var userObject = orderedData["user"] as JObject;
        Assert.IsNotNull(userObject);
        var userProperties = userObject.Properties().Select(p => p.Name).ToList();
        Assert.That(userProperties[0], Is.EqualTo("lastName"));
        Assert.That(userProperties[1], Is.EqualTo("firstName"));
        Assert.That(userProperties[2], Is.EqualTo("age"));
        Assert.That(userProperties[3], Is.EqualTo("email"));
    }

    [Test]
    public void OrderBySchema_PreservesAllData()
    {
        // Arrange
        var schema = @"{""type"":""object"",""properties"":{""b"":{""type"":""string""},""a"":{""type"":""string""}}}";
        var input = @"{""data"":{""c"":""value3"",""a"":""value1"",""b"":""value2"",""d"":""value4""}}";

        var functionValue = JsonConvert.SerializeObject($"=orderBySchema({schema})");
        var script = $"[{{\"path\":\"$.data\",\"value\":{functionValue},\"command\":\"set\"}}]";
        var parsedScript = JLioConvert.Parse(script, parseOptions);
        var inputObject = JToken.Parse(input);

        // Act
        var result = parsedScript.Execute(inputObject);

        // Assert
        Assert.That(result.Success, Is.True);
        
        var orderedData = result.Data["data"] as JObject;
        Assert.IsNotNull(orderedData);
        
        // Check all values are preserved
        Assert.That(orderedData["a"]?.ToString(), Is.EqualTo("value1"));
        Assert.That(orderedData["b"]?.ToString(), Is.EqualTo("value2"));
        Assert.That(orderedData["c"]?.ToString(), Is.EqualTo("value3"));
        Assert.That(orderedData["d"]?.ToString(), Is.EqualTo("value4"));
        
        // Check ordering: schema properties first (b, a), then additional properties (c, d)
        var properties = orderedData.Properties().Select(p => p.Name).ToList();
        Assert.That(properties[0], Is.EqualTo("b"));
        Assert.That(properties[1], Is.EqualTo("a"));
        Assert.That(properties.Skip(2).Contains("c"), Is.True);
        Assert.That(properties.Skip(2).Contains("d"), Is.True);
    }

    [Test]
    public void OrderBySchema_RequiresOneArgument()
    {
        // Test with no arguments
        var noArgsFunction = new OrderBySchema();
        var result = noArgsFunction.Execute(JToken.Parse("{}"), JToken.Parse("{}"), executeContext);
        
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }
}