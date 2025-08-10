using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Math.Builders;
using JLio.Extensions.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class MinTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=min(1,2,3)", "{}", 1)]
    [TestCase("=min(5,2,8)", "{}", 2)]
    [TestCase("=min(-1,2,3)", "{}", -1)]
    [TestCase("=min($.a,$.b,$.c)", "{\"a\":5,\"b\":2,\"c\":8}", 2)]
    [TestCase("=min($.numbers[*])", "{\"numbers\":[4,6,1,9]}", 1)]
    [TestCase("=min($.value)", "{\"value\":42}", 42)]
    public void MinTests_ValidInputs(string function, string data, double resultValue)
    {
        var script = $"{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}";
        var scriptArray = $"[{script}]";
        var result = JLioConvert.Parse(scriptArray, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Min_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(MinBuilders.Min("1", "5", "3"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Min_FailsWithNoArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=min()\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}