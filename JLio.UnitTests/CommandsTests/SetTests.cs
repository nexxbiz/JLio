using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using JLio.Functions.Builders;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.CommandsTests;

public class SetTests
{
    private JToken data;
    private FunctionConverter functionConverter;
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        functionConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
        executeOptions = ExecutionContext.CreateDefault();
        data = JToken.Parse(
            "{\r\n  \"myString\": \"demo2\",\r\n  \"myNumber\": 2.2,\r\n  \"myInteger\": 20,\r\n  \"myObject\": {\r\n    \"myObject\": {\"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]},\r\n    \"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]\r\n  },\r\n  \"myArray\": [\r\n    2,\r\n    20,\r\n    200,\r\n    2000\r\n  ],\r\n  \"myBoolean\": true,\r\n  \"myNull\": null\r\n}");
    }

    [TestCase("$.myObject.myArray", "newData")]
    [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
    [TestCase("$.myArray", "newData")]
    [TestCase("$.myNull", "newData")]
    [TestCase("$..myArray", "newData")]
    [TestCase("$.myString", "newData")]
    public void CanSetValues(string path, string value)
    {
        var valueToSet = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Set(path, valueToSet).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [TestCase("$.myObject", "newData")]
    [TestCase("$.myArray", "newData")]
    [TestCase("$..myObject", "newData")]
    [TestCase("$..myArray", "newData")]
    [TestCase("$.myString", "newData")]
    public void CanSetCorrectValues(string path, string value)
    {
        var valueToSet = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Set(path, valueToSet).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
        Assert.IsTrue(data.SelectTokens(path).All(t => t.Value<string>() == "newData"));
    }

    [TestCase("$.myObject", "")]
    [TestCase("$.myArray", "")]
    [TestCase("$..myObject", "")]
    [TestCase("$..myArray", "")]
    [TestCase("$.myString", "")]
    public void CanSetCorrectValuesEmptyString(string path, string value)
    {
        var valueToSet = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Set(path, valueToSet).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
        Assert.IsTrue(data.SelectTokens(path).All(t => t.Value<string>() == ""));
    }

    [TestCase("", "newData", "Path property for set command is missing")]
    [TestCase("", null, "Path property for set command is missing")]
    public void CanExecuteWithArgumentsNotProvided(string path, string value, string message)
    {
        var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Set(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().Any(l => l.Message == message));
    }

    [Test]
    public void CanUseFluentApi()
    {
        var data = JObject.Parse("{ \"demo\" : \"old value\" , \"demo2\" : \"old value\" }");
        var script = new JLioScript()
                .Set(new JValue("new Value"))
                .OnPath("$.demo")
                .Set(DatetimeBuilders.Datetime())
                .OnPath("$.demo2")
            ;
        var result = script.Execute(data);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreNotEqual(result.Data.SelectToken("$.demo").Type, JTokenType.Null);
        Assert.AreEqual(result.Data.SelectToken("$.demo").Value<string>(), "new Value");
        Assert.AreNotEqual(result.Data.SelectToken("$.demo2").Type, JTokenType.Null);
    }
}