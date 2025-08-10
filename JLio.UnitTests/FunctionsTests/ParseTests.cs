using System.Linq;
using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.JSchema;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.Text.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class ParseTests
{
    private IExecutionContext executeContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterText().RegisterMath();
        parseOptions.RegisterFunction<FilterBySchema>();
        executeContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=parse()", "{\"result\" : \"3\" }", 3)]
    [TestCase("=parse()", "{\"result\" : \"\\\"3\\\"\"}", "\"3\"")]
    [TestCase("=parse()", "{\"result\" : \"{\\\"demo\\\":67}\"}", "{\"demo\":67}")]
    public void ScriptTestSet(string function, string data, object expectedResult)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult.ToString() ?? string.Empty),
            result.Data.SelectToken("$.result")));
    }

    [TestCase("=parse($.item)", "{\"item\" : \"3\" }", "3")]
    [TestCase("=parse($.item)", "{\"item\" : \"\\\"3\\\"\"}", "\"3\"")]
    [TestCase("=parse($.item)", "{\"item\" : \"{\\\"demo\\\":67}\"}", "{\"demo\": 67}")]
    public void ScriptTestAdd(string function, string data, string expectedResult)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.IsTrue(JToken.DeepEquals(result.Data.SelectToken("$.result"),
            JToken.Parse(expectedResult)));
    }

    [TestCase("=parse($.item)", "{\"result\" : \"{\\\"demo :67}\"}")]
    public void ScriptTestAddFaultyString(string function, string data)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }

    [TestCase("=parse()", "{\"result\" : {\"demo\" :67}}")]
    public void ParseOnNonStringTypeString(string function, string data)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CanbeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Set(ParseBuilders.Parse())
                .OnPath("$.id")
                .Add(ParseBuilders.Parse("$.id"))
                .OnPath("$.result")
            ;
        var result = script.Execute(JObject.Parse("{\"id\" : \"3\" }"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreNotEqual(result.Data.SelectToken("$.result")?.Type, JTokenType.Null);
        Assert.AreEqual(result.Data.SelectToken("$.result").Value<int>(), 3);
    }
}