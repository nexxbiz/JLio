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

public class ModuloTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=modulo(10, 3)", "{}", 1)]
    [TestCase("=modulo(20, 6)", "{}", 2)]
    [TestCase("=modulo(5, 5)", "{}", 0)]
    [TestCase("=modulo(7, 2)", "{}", 1)]
    [TestCase("=modulo(100, 7)", "{}", 2)]
    [TestCase("=modulo(15, 4)", "{}", 3)]
    [TestCase("=modulo(0, 5)", "{}", 0)]
    [TestCase("=modulo(10.5, 3)", "{}", 1.5)]
    [TestCase("=modulo(-10, 3)", "{}", -1)]
    [TestCase("=modulo(10, -3)", "{}", 1)]
    [TestCase("=modulo(-10, -3)", "{}", -1)]
    [TestCase("=modulo($.dividend, $.divisor)", "{\"dividend\":17,\"divisor\":5}", 2)]
    public void ModuloTests_ValidInputs(string function, string data, double resultValue)
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
    public void Modulo_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(ModuloBuilders.Modulo("17", "5"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Modulo_FailsWithOneArgument()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=modulo(10)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Modulo_FailsWithThreeArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=modulo(10,3,2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Modulo_FailsWithDivisionByZero()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=modulo(10,0)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error && i.Message.Contains("divisor cannot be zero")));
    }
}
