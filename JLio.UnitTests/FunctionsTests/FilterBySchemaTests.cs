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
public class FilterBySchemaTests
{
    [SetUp]
    public void Setup()
    {
        parseOptions = new ParseOptions();
        var commandsProvider = new CommandsProvider();
        commandsProvider.Register<Set>();
        parseOptions.JLioCommandConverter = new CommandConverter(commandsProvider);
        var functionsProvider = new FunctionsProvider();
        functionsProvider.Register<FilterBySchema>();
        parseOptions.JLioFunctionConverter = new FunctionConverter(functionsProvider);
        executeContext = ExecutionContext.CreateDefault();
    }

    private IExecutionContext executeContext;
    private ParseOptions parseOptions;

    [Test]
    [TestCase(
        "{\"type\":\"object\",\"description\":\"The point of interest location description.\",\"properties\":{\"pageIndex\":{\"type\":\"number\",\"description\":\"The page index to view\"},\"points\":{\"description\":\"Used if viewState.zoomWidth is true; specifies 4 corners of the rectangle to make visible.\",\"type\":\"string\"},\"something\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"loc\":{\"type\":\"string\"},\"toll\":{\"type\":\"string\"},\"message\":{\"type\":\"string\"}},\"required\":[\"loc\"]}},\"viewState\":{\"type\":\"object\",\"description\":\"The view state information\",\"properties\":{\"hasViewState\":{\"type\":\"boolean\",\"description\":\"Whether there is view state information to use\"},\"rotate\":{\"type\":\"number\",\"description\":\"Degrees of rotation\"},\"extents\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit extents or not\"},\"zoomWidth\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit width or not\"},\"scaleFactor\":{\"type\":\"number\",\"description\":\"The scale factor for the view state\"},\"deviceRect\":{\"type\":\"object\",\"description\":\"The device rectangle geometry\",\"properties\":{\"left\":{\"type\":\"number\",\"description\":\"The left edge of the device rectangle\"},\"right\":{\"type\":\"number\",\"description\":\"The right edge of the device rectangle\"},\"top\":{\"type\":\"number\",\"description\":\"The top edge of the device rectangle\"},\"bottom\":{\"type\":\"number\",\"description\":\"The bottom edge of the device rectangle\"}}},\"eyePoint\":{\"type\":\"object\",\"description\":\"The eyepoint coordinate\",\"properties\":{\"x\":{\"type\":\"number\",\"description\":\"The x coordinate of the eyepoint\"},\"y\":{\"type\":\"number\",\"description\":\"The y coordinate of the eyepoint\"}}}}}}}")]
    public void CanBeUsedInFluentApi(string schema)
    {
        var parsedSchema = JSchema.Parse(schema);

        var script = new JLioScript()
            .Set(FilterBySchemaBuilders.FilterBySchema(parsedSchema))
            .OnPath("$.demo");
        var result = script.Execute(JToken.Parse(
            "{\"demo\":{\"points\":\"dolor Excepteur aliquip in\",\"viewState\":{\"sintc\":false,\"velit_c3\":\"dolore Ut\",\"aliquab84\":98258463,\"tempor_0\":true,\"amet_54\":true,\"sunt_c\":\"exercitation Excepteur ipsum\",\"ex_16\":9425019.714207217,\"do_11e\":false},\"something\":[{\"loc\":\"dolor et Excepteur exercitation\",\"toll\":\"ad elit esse adipisicing\",\"message\":\"tempor est\"},{\"loc\":\"veniam do fugiat labore eiusmod\",\"toll\":\"dolore aliqua veniam sit\",\"message\":\"aliquip ad do irure velit\"},{\"loc\":\"et\"},{\"loc\":\"dolore non dolore sint\",\"toll\":\"minim ut consectetur fugiat\",\"message\":\"cillum exercitation quis id laborum\"}]  }}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void CanBeUsedInFluentApi_SchemaPath()
    {
        var script = new JLioScript()
            .Set(FilterBySchemaBuilders.FilterBySchema("$.schema"))
            .OnPath("$.demo");
        var result = script.Execute(JToken.Parse(
            "{\"schema\":{\"type\":\"object\",\"description\":\"The point of interest location description.\",\"properties\":{\"pageIndex\":{\"type\":\"number\",\"description\":\"The page index to view\"},\"points\":{\"description\":\"Used if viewState.zoomWidth is true; specifies 4 corners of the rectangle to make visible.\",\"type\":\"string\"},\"something\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"loc\":{\"type\":\"string\"},\"toll\":{\"type\":\"string\"},\"message\":{\"type\":\"string\"}},\"required\":[\"loc\"]}},\"viewState\":{\"type\":\"object\",\"description\":\"The view state information\",\"properties\":{\"hasViewState\":{\"type\":\"boolean\",\"description\":\"Whether there is view state information to use\"},\"rotate\":{\"type\":\"number\",\"description\":\"Degrees of rotation\"},\"extents\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit extents or not\"},\"zoomWidth\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit width or not\"},\"scaleFactor\":{\"type\":\"number\",\"description\":\"The scale factor for the view state\"},\"deviceRect\":{\"type\":\"object\",\"description\":\"The device rectangle geometry\",\"properties\":{\"left\":{\"type\":\"number\",\"description\":\"The left edge of the device rectangle\"},\"right\":{\"type\":\"number\",\"description\":\"The right edge of the device rectangle\"},\"top\":{\"type\":\"number\",\"description\":\"The top edge of the device rectangle\"},\"bottom\":{\"type\":\"number\",\"description\":\"The bottom edge of the device rectangle\"}}},\"eyePoint\":{\"type\":\"object\",\"description\":\"The eyepoint coordinate\",\"properties\":{\"x\":{\"type\":\"number\",\"description\":\"The x coordinate of the eyepoint\"},\"y\":{\"type\":\"number\",\"description\":\"The y coordinate of the eyepoint\"}}}}}}},\"demo\":{\"pageIndex\":5,\"shouldBeRemoved\":true}}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [Test]
    [TestCase(
        "=filterBySchema($.schema)",
        "{\"result\" : [1,2] , \"schema\" : {\"$id\":\"https://example.com/person.schema.json\",\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"title\":\"Person\",\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\",\"description\":\"The person's first name.\"},\"lastName\":{\"type\":\"string\",\"description\":\"The person's last name.\"},\"age\":{\"description\":\"Age in years which must be equal to or greater than zero.\",\"type\":\"integer\",\"minimum\":0}}}    }")]
    public void WillReturnErrorFalse(string function, string data)
    {
        var script = $"[{{\"path\":\"$.result[*]\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    [TestCase(
        "{\"$id\":\"https://example.com/person.schema.json\",\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"title\":\"Person\",\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\",\"description\":\"The person's first name.\"},\"lastName\":{\"type\":\"string\",\"description\":\"The person's last name.\"},\"age\":{\"description\":\"Age in years which must be equal to or greater than zero.\",\"type\":\"integer\",\"minimum\":0}}}",
        "{\"result\" : [1,2]}")]
    public void WillReturnErrorFalseExecute(string function, string data)
    {
        var functionValue =
            JsonConvert.SerializeObject($"=filterBySchema({function})");
        var script = $"[{{\"path\":\"$.result[*]\",\"value\":{functionValue},\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    [TestCase(
        "{\r\n    \"$schema\": \"http://json-schema.org/draft-07/schema#\",\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"transactionInfo\": {\r\n            \"type\": \"object\",\r\n            \"properties\": {\r\n                \"policy\": {\r\n                    \"type\": \"array\",\r\n                    \"items\": {\r\n                        \"type\": \"object\",\r\n                        \"properties\": {\r\n                            \"exceptionHandlingInfo\": {\r\n                                \"type\": [\r\n                                    \"object\",\r\n                                    \"null\"\r\n                                ],\r\n                                \"properties\": {\r\n                                    \"exceptionTicketUniqueIdentifier\": {\r\n                                        \"type\": \"string\"\r\n                                    },\r\n                                    \"alterationAllowed\": {\r\n                                        \"type\": \"boolean\"\r\n                                    },\r\n                                    \"bypassAllowed\": {\r\n                                        \"type\": \"boolean\"\r\n                                    },\r\n                                    \"exceptionHandlingAllowed\": {\r\n                                        \"type\": \"boolean\"\r\n                                    }\r\n                                },\r\n                                \"required\": [\r\n                                    \"alterationAllowed\",\r\n                                    \"bypassAllowed\",\r\n                                    \"exceptionHandlingAllowed\"\r\n                                ]\r\n                            }\r\n                        },\r\n                        \"required\": [\r\n                            \"exceptionHandlingInfo\"\r\n                        ]\r\n                    }\r\n                }\r\n            }\r\n        }\r\n    }\r\n}",
        "{\"result\" :  {\"transactionInfo\":{\"policy\":[{\"processingCode\":\"0\",\"refKey\":\"0001_Policy\",\"contractNumber\":\"\",\"exceptionHandlingInfo\":{\"exceptionTicketUniqueIdentifier\":\"45cca6d6b120490284d995b5b0e2a291\",\"exceptionHandlingAllowed\":true,\"bypassAllowed\":true,\"alterationAllowed\":false},\"productProcessingInfo\":null,\"contractProcessingInfo\":null,\"processLockInfo\":{\"enabled\":false}}]}}   }")]
    public void WillBeUntoutched(string function, string data)
    {
        var endResult = data;
        var functionValue =
            JsonConvert.SerializeObject($"=filterBySchema({function})");
        var script = $"[{{\"path\":\"$.result[*]\",\"value\":{functionValue},\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(endResult).SelectToken("$.result"), result.Data.SelectToken("$.result")));
        Assert.IsFalse(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }


    [Test]
    public void CanFilterPropertiesOfArrayItems()
    {
        // Arrange
        var filterBySchemaScript = "[{\"path\":\"$.result\",\"value\":\"=filterBySchema($.schema)\",\"command\":\"set\"}]";
        var input = "{\"result\":{\"policies\":[{\"country\":\"0\",\"sequenceNumber\":1,\"distributionType\":\"LKA\",\"statusType\":\"1\",\"TimeStamp\":\"1899-12-31\",\"processingCode\":\"4\",\"partyRef\":[\"1882937\",\"9928353\"]},{\"country\":\"0\",\"sequenceNumber\":1,\"distributionType\":\"ASD*\",\"statusType\":\"1\",\"TimeStamp\":\"1899-12-31\",\"processingCode\":\"4\",\"partyRef\":[\"1882937\",\"9928353\"]}]},\"schema\":{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"policies\":{\"type\":\"array\",\"items\":{\"properties\":{\"sequenceNumber\":{\"type\":\"integer\"},\"processingCode\":{\"type\":\"string\"},\"partyRef\":{\"type\":\"array\",\"items\":{\"format\":\"\",\"type\":\"string\"}}},\"type\":\"object\"}}}}}";
        var parsedScript = JLioConvert.Parse(filterBySchemaScript, parseOptions);
        var inputObject = JToken.Parse(input);

        // Act
        var result = parsedScript.Execute(inputObject);

        // Assert
        Assert.That(result.Success, Is.True);

        var policies = result.Data["result"]!["policies"]!.Select(x => (JObject)x);
        foreach (var policy in policies)
        {
            Assert.That(policy, Is.Not.Null);

            // Assert number of keys
            Assert.That(policy.Count, Is.EqualTo(3));

            // Assert keys
            Assert.That(policy.ContainsKey("sequenceNumber"), Is.True);
            Assert.That(policy["sequenceNumber"], Is.Not.Null);

            Assert.That(policy.ContainsKey("processingCode"), Is.True);
            Assert.That(policy["processingCode"], Is.Not.Null);

            Assert.That(policy.ContainsKey("partyRef"), Is.True);
            Assert.That(policy["partyRef"], Is.Not.Null);
            Assert.That(policy["partyRef"].Count(), Is.GreaterThan(1));
        }
    }
}
