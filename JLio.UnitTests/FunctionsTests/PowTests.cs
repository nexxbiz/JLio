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

public class PowTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=pow(2, 3)", "{}", 8)]
    [TestCase("=pow(5, 2)", "{}", 25)]
    [TestCase("=pow(2, 0)", "{}", 1)]
    [TestCase("=pow(0, 5)", "{}", 0)]
    [TestCase("=pow(4, 0.5)", "{}", 2)]
    [TestCase("=pow(-2, 3)", "{}", -8)]
    [TestCase("=pow(-2, 2)", "{}", 4)]
    [TestCase("=pow($.base, $.exp)", "{\"base\":3,\"exp\":4}", 81)]
    [TestCase("=pow(10, -2)", "{}", 0.01)]
    public void PowTests_ValidInputs(string function, string data, double resultValue)
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
    public void Pow_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(PowBuilders.Pow("2", "8"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(256, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Pow_FailsWithOneArgument()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow(2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Pow_FailsWithThreeArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow(2,3,4)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}