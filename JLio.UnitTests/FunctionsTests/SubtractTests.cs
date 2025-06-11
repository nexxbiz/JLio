using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Math.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class SubtractTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=subtract(10,3)", "{}", 7)]
    [TestCase("=subtract('10','3')", "{}", 7)]
    [TestCase("=subtract($.a,$.b)", "{\"a\":5,\"b\":2}", 3)]
    [TestCase("=subtract($.a,$.b)", "{\"a\":null,\"b\":2}", -2)]
    [TestCase("=subtract($.values[*],$.subs[*])", "{\"values\":[5,3],\"subs\":[2,1]}", 5)]
    [TestCase("=subtract($.numbers[*],2)", "{\"numbers\":[2,3,4]}", 7)]
    [TestCase("=subtract($.nested,$.b[*])", "{\"nested\":[1,[2,3]],\"b\":[1]}", 5)]
    [TestCase("=subtract($.sa,$.sb)", "{\"sa\":\"10\",\"sb\":\"8\"}", 2)]
    public void subtractTests(string function, string data, double resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(SubtractBuilders.Subtract("5", "2"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void ScriptFailsOnInvalidValue()
    {
        var script = "[{'path':'$.result','value':'=subtract($.obj,$.a)','command':'add'}]".Replace("'","\"");
        var data = "{\"obj\":{\"v\":1},\"a\":2}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void ScriptFailsOnWrongArgumentCount()
    {
        var script = "[{'path':'$.result','value':'=subtract($.a)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"a\":1}"), executionContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}
