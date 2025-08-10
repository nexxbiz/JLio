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

public class FloorTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=floor(3.14)", "{}", 3)]
    [TestCase("=floor(3.99)", "{}", 3)]
    [TestCase("=floor(-2.3)", "{}", -3)]
    [TestCase("=floor(-2.9)", "{}", -3)]
    [TestCase("=floor(5)", "{}", 5)]
    [TestCase("=floor(0)", "{}", 0)]
    [TestCase("=floor($.value)", "{\"value\":3.75}", 3)]
    [TestCase("=floor($.value)", "{\"value\":-1.25}", -2)]
    public void FloorTests_ValidInputs(string function, string data, double resultValue)
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
    public void Floor_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(FloorBuilders.Floor("3.99"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Floor_FailsWithMultipleArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=floor(1,2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}