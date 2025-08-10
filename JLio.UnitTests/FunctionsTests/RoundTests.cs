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

public class RoundTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=round(3.14)", "{}", 3)]
    [TestCase("=round(3.75)", "{}", 4)]
    [TestCase("=round(-2.3)", "{}", -2)]
    [TestCase("=round(-2.7)", "{}", -3)]
    [TestCase("=round(3.14159, 2)", "{}", 3.14)]
    [TestCase("=round(3.14159, 3)", "{}", 3.142)]
    [TestCase("=round(1234.5678, 1)", "{}", 1234.6)]
    [TestCase("=round($.value, 2)", "{\"value\":3.14159}", 3.14)]
    [TestCase("=round($.value, $.decimals)", "{\"value\":3.14159,\"decimals\":3}", 3.142)]
    public void RoundTests_ValidInputs(string function, string data, double resultValue)
    {
        var script = $"{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}";
        var scriptArray = $"[{script}]";
        var result = JLioConvert.Parse(scriptArray, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.That(result.Data.SelectToken("$.result")?.Value<double>(), Is.EqualTo(resultValue).Within(0.0001));
    }

    [Test]
    public void Round_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(RoundBuilders.Round("3.14159", "2"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.SelectToken("$.result")?.Value<double>(), Is.EqualTo(3.14).Within(0.001));
    }

    [Test]
    public void Round_CanBeUsedWithSingleArgument()
    {
        var script = new JLioScript()
                .Add(RoundBuilders.Round("3.75"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(4, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Round_FailsWithTooManyArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=round(1,2,3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Round_FailsWithInvalidDecimals()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=round(3.14, -1)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}