using System;
using System.Linq;
using JLio.Client;
using JLio.Commands.Advanced.Builders;
using JLio.Commands.Builders;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.ScriptTextHandling;

public class TextHandling
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]")]
    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=datetime(datetime(UTC))\",\"command\":\"add\"}]")]
    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=newGuid()\",\"command\":\"add\"}]")]
    [TestCase(
        "[{\"path\":\"$.myObject.newProperty\",\"value\":\"=concat('fixed',@.localPath,$.rootPath,datetime(UTC))\",\"command\":\"add\"}]")]
    [TestCase(
        "[{\"path\": \"$.myObject.newProperty\",\"value\": { \"new object\": \"Added by value\" },\"command\": \"add\"}]")]
    [TestCase(
        "[{\"path\":\"$.myObject.newProperty\",\"value\":\"=concat(datetime(UTC,'dd-MM-yyyy'),'-date')\",\"command\":\"add\"},{\"path\":\"$.myObject.myDate\",\"value\":\"=datetime('UTC','dd-MM-yyyy')\",\"command\":\"add\"}]")]
    public void CanParseAndSerializeScript(string script)
    {
        var jlioScript = JLioConvert.Parse(script);
        var scriptText2 = JLioConvert.Serialize(jlioScript);
        var result = jlioScript.Execute(new JObject());
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(script), JToken.Parse(scriptText2)));
        Assert.IsTrue(result.Success);
    }

    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"unknown\"}]")]
    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]")]
    [TestCase("[{\"path\": \"$.myObject.newProperty\",\"value\": 1,\"command\": \"add\"}]")]
    [TestCase(
        "[{\"path\": \"$.myObject.newProperty\",\"value\": { \"new object\": \"Added by value\" },\"command\": \"add\"}]")]
    public void CanParse(string scriptText)
    {
        Assert.DoesNotThrow(() => { JLioConvert.Parse(scriptText); }
        );
    }

    [TestCase(@"[{""command"": ""decisionTable"", ""path"": ""$.customer"", ""decisionTable"": {""inputs"": [{""name"": ""age"", ""path"": ""@.age""}], ""outputs"": [{""name"": ""category"", ""path"": ""@.category""}], ""rules"": [{""conditions"": {""age"": "">=18""}, ""results"": {""category"": ""adult""}}]}}]")]
    [TestCase(@"[{""command"": ""decisionTable"", ""path"": ""$.product"", ""decisionTable"": {""inputs"": [{""name"": ""price"", ""path"": ""@.price""}], ""outputs"": [{""name"": ""tier"", ""path"": ""@.tier""}], ""rules"": [{""conditions"": {""price"": "">100""}, ""results"": {""tier"": ""premium""}}], ""defaultResults"": {""tier"": ""standard""}}}]")]
    [TestCase(@"[{""command"": ""decisionTable"", ""path"": ""$.order"", ""decisionTable"": {""inputs"": [{""name"": ""total"", ""path"": ""@.total""}], ""outputs"": [{""name"": ""discount"", ""path"": ""@.discount""}], ""rules"": [{""priority"": 1, ""conditions"": {""total"": "">=50""}, ""results"": {""discount"": 0.1}}]}}]")]
    public void CanParseDescisionTable(string scriptText)
    {
        Assert.DoesNotThrow(() => { JLioConvert.Parse(scriptText); });
    }

    [TestCase("", "{\"myObject\":{\"initialProperty\":\"initial value\"}}")]
    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=newGuid()\",\"command\":\"add\"}]",
        "{\"myObject\":{\"initialProperty\":\"initial value\"}}")]
    public void CanParseAndExecute(string scriptText, string data)
    {
        var script = JLioConvert.Parse(scriptText);
        var result = script.Execute(JToken.Parse(data));

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
    }

    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"unknown\"}]", "{}")]
    public void CanParseAndExecuteWithUnknownCommand(string scriptText, string data)
    {
        var script = JLioConvert.Parse(scriptText);
        var result = script.Execute(JToken.Parse(data));

        Assert.IsFalse(result.Success);
        Assert.IsFalse(script.Validate());
        Assert.IsTrue(script.GetValidationResults().Any());
        Assert.IsNotNull(result.Data);
    }

    [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"unknown\"}]", "{}")]
    public void CanParseAndExecuteWithLogging(string scriptText, string data)
    {
        var context = ExecutionContext.CreateDefault();
        var script = JLioConvert.Parse(scriptText);
        var result = script.Execute(JToken.Parse(data), context);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(context.GetLogEntries().Count(i => i.Level == LogLevel.Error) == 1);
        Assert.IsTrue(
            context.GetLogEntries().All(i => i.DateTime < DateTime.Now && i.DateTime != new DateTime()));
        Assert.IsTrue(context.GetLogEntries().All(i => i.Level != LogLevel.None));
        Assert.IsFalse(context.GetLogEntries().Any(i => string.IsNullOrEmpty(i.Group)));
        Assert.IsTrue(context.GetLogEntries().Start < DateTime.Now);
        Assert.IsTrue(context.GetLogEntries().End < DateTime.Now);
        Assert.IsTrue(context.GetLogEntries().ExecutionTimeMilliseconds >= 0);
        Assert.IsNotNull(result.Data);
    }

    [Test]
    public void CanSerializeAndDeserializeScriptForCommands()
    {
        var script = new JLioScript()
            .Add(new JValue(0)).OnPath("$.demo")
            .Set(new JValue(1)).OnPath("$.demo")
            .Put(new JValue(1)).OnPath("$.demo")
            .Move("$.demo").To("$.otherDemo")
            .Copy("$.otherDemo").To(" $.demo")
            .Compare("$.first").With("$.second").SetResultOn("$.result")
            .Merge("$.first").With("$.second").UsingDefaultSettings()
            .Remove("$.demo");
        var scriptText = string.Empty;
        Assert.DoesNotThrow(() => { scriptText = JLioConvert.Serialize(script); });
        Assert.IsFalse(string.IsNullOrEmpty(scriptText));
        var scriptText2 = JLioConvert.Serialize(JLioConvert.Parse(scriptText));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(scriptText), JToken.Parse(scriptText2)));
    }

    [TestCase("$.ite", 4)]
    public void GetIntellisense(string text, int numberOfItems)
    {
        var dataObject = JToken.Parse("{\"item1\":1,\"item11\":11,\"item111\":111,\"item1111\":1111}");
        var intellisense = JsonPathMethods.GetIntellisense(text, dataObject, new JsonPathItemsFetcher());
        Assert.AreEqual(numberOfItems, intellisense.Count);
    }

    [TestCase("01")]           // Leading zero
    [TestCase("007")]          // Multiple leading zeros  
    [TestCase("A123")]         // Letter prefix with numbers
    [TestCase("123B")]         // Numbers with letter suffix
    [TestCase("ID-123")]       // Alphanumeric with separator
    [TestCase("+31612345678")] // Phone number format
    [TestCase("1.0.0")]        // Version number
    [TestCase("2023-01-01")]   // Date format
    [TestCase("ABC123DEF")]    // Mixed alphanumeric
    [TestCase("00100")]        // Leading zeros with trailing zeros
    public void StringContainingNumbersPreservesStringType(string stringValue)
    {
        var input = JToken.Parse("{\"sample\":{}}");
        var scriptText = $"[{{\"path\":\"$.sample.newProperty\",\"value\":\"{stringValue}\",\"command\":\"add\"}}]";
        
        var script = JLioConvert.Parse(scriptText);
        var result = script.Execute(input);
        
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        
        var newProperty = result.Data.SelectToken("$.sample.newProperty");
        Assert.IsNotNull(newProperty);
        Assert.AreEqual(JTokenType.String, newProperty.Type);
        Assert.AreEqual(stringValue, newProperty.Value<string>());
    }
}