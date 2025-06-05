using System.Linq;
using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions.Builders;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class AddTests
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

    [TestCase("$.myObject.newItem", "newData")]
    [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
    [TestCase("$.myArray", "newData")]
    [TestCase("$.myNull", "newData")]
    [TestCase("$..myObject.newItem", "newData")]
    [TestCase("$..myArray", "newData")]
    [TestCase("$.newProperty", "newData")]
    [TestCase("$..myObject[?(@.myArray)].newProperty)",
        "newData")] // this is not working yet  need to consult newtonsoft
    public void CanAddValues(string path, string value)
    {
        var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Add(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [TestCase("$.myObject.newItem", "newData")]
    [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
    [TestCase("$.myArray", "newData")]
    [TestCase("$..myObject.newItem", "newData")]
    [TestCase("$..myArray", "newData")]
    [TestCase("$.newProperty", "newData")]
    public void CanAddCorrectValues(string path, string value)
    {
        var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Add(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
    }

    [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
    public void CanAddCorrectValuesWithOtherConstructor(string path, string value)
    {
        var result = new Add(path, new JValue(value)).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
    }

    [TestCase("$.myObject.newItem", "newData")]
    public void CanAddCorrectValuesWithEmptyConstructor(string path, string value)
    {
        var command = new Add
        { Path = path, Value = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter)) };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
    }

    [TestCase("$.newProperty", "{\"demo\" : 3}")]
    public void CanAddCorrectValuesAsTokens(string path, string value)
    {
        var tokenToAdd = JToken.Parse(value);
        var valueToAdd = new FunctionSupportedValue(new FixedValue(tokenToAdd, functionConverter));
        var result = new Add(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
        Assert.IsTrue(JToken.DeepEquals(data.SelectToken(path), tokenToAdd));
    }

    [TestCase("", "newData", "Path property for add command is missing")]
    [TestCase("", null, "Path property for add command is missing")]
    public void CanExecuteWithArgumentsNotProvided(string path, string value, string message)
    {
        var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value), functionConverter));
        var result = new Add(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().Any(l => l.Message == message));
    }

    //[TestCase("$.newProperty", "{\"demo\" : \"=newGuid()\"}")]
    //[TestCase("$.newProperty", "{\"demo\" : \"=concat($.myString, '-demo-', newGuid())\", \"demo2\" : \"=concat($.myString, '-demo2-', newGuid())\"}")]
    [TestCase("$.newProperty", "{\"newprop\" : { \"newSubProp\" : 56},   \"demo\" : \"=fetch($.myObject.myArray)\", \"demo2\" : { \"prop\": \"=concat($.myString, '-demo2-', newGuid())\", \"demo2\" : \"=fetch($.myArray[1])\"}}")]
    public void CanAddCorrectValuesAsFunctions(string path, string value)
    {
        var tokenToAdd = JToken.Parse(value);
        FunctionConverter functionsConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
        var valueToAdd = new FunctionSupportedValue(new FixedValue(tokenToAdd, functionsConverter));
        var result = new Add(path, valueToAdd).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
        Assert.IsTrue(data.SelectTokens(path).Any());
        // it should not be the same as the tokenToAdd, because the function is executed
        Assert.IsFalse(JToken.DeepEquals(data.SelectToken(path), tokenToAdd));
    }

    [Test]
    public void CanUseFluentApi()
    {
        var script = new JLioScript()
                .Add(new JValue("new Value"))
                .OnPath("$.demo")
                .Add(DatetimeBuilders.Datetime())
                .OnPath("$.this.is.a.long.path.with.a.date")
            ;
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreNotEqual(result.Data.SelectToken("$.demo").Type, JTokenType.Null);
        Assert.AreNotEqual(result.Data.SelectToken("$.this.is.a.long.path.with.a.date").Type, JTokenType.Null);
    }
}