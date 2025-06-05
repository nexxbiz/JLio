using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class SumTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=sum()", "{}", 0)]
    [TestCase("=sum(1,2,3)", "{}", 6)]
    [TestCase("=sum($.a,$.b,$.c)", "{\"a\":1,\"b\":2,\"c\":3}", 6)]
    public void ScriptTest(string function, string data, double resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void ScriptFailsOnInvalidValue()
    {
        var script = "[{'path':'$.result','value':'=sum($.a)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"a\":\"x\"}"), executionContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(SumBuilders.Sum("1", "2"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<double>());
    }
}
