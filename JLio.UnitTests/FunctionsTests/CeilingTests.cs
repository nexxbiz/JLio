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

public class CeilingTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=ceiling(3.14)", "{}", 4)]
    [TestCase("=ceiling(3.01)", "{}", 4)]
    [TestCase("=ceiling(-2.3)", "{}", -2)]
    [TestCase("=ceiling(-2.9)", "{}", -2)]
    [TestCase("=ceiling(5)", "{}", 5)]
    [TestCase("=ceiling(0)", "{}", 0)]
    [TestCase("=ceiling($.value)", "{\"value\":3.25}", 4)]
    [TestCase("=ceiling($.value)", "{\"value\":-1.75}", -1)]
    public void CeilingTests_ValidInputs(string function, string data, double resultValue)
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
    public void Ceiling_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(CeilingBuilders.Ceiling("3.01"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(4, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Ceiling_FailsWithMultipleArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=ceiling(1,2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void Ceiling_WithCalculateForDivisionExpression()
    {
        var data = "{\"totalItems\": 157, \"batchSize\": 25}";
        var script = "[{\"path\":\"$.result\",\"value\":\"=ceiling(calculate('{{$.totalItems}} / {{$.batchSize}}'))\",\"command\":\"add\"}]";
        
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(7, result.Data.SelectToken("$.result")?.Value<double>());
    }
}