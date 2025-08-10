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

public class SqrtTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=sqrt(4)", "{}", 2)]
    [TestCase("=sqrt(9)", "{}", 3)]
    [TestCase("=sqrt(16)", "{}", 4)]
    [TestCase("=sqrt(0)", "{}", 0)]
    [TestCase("=sqrt(1)", "{}", 1)]
    [TestCase("=sqrt(2)", "{}", 1.4142)]
    [TestCase("=sqrt($.value)", "{\"value\":25}", 5)]
    [TestCase("=sqrt($.value)", "{\"value\":6.25}", 2.5)]
    public void SqrtTests_ValidInputs(string function, string data, double resultValue)
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
    public void Sqrt_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(SqrtBuilders.Sqrt("64"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(8, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Sqrt_FailsWithNegativeNumber()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=sqrt(-4)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Sqrt_FailsWithMultipleArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=sqrt(4,9)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}