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

public class MedianTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=median(1,2,3)", "{}", 2)]
    [TestCase("=median(1,2,3,4)", "{}", 2.5)]
    [TestCase("=median(5,1,3,2,4)", "{}", 3)]
    [TestCase("=median(-1,0,1)", "{}", 0)]
    [TestCase("=median($.numbers[*])", "{\"numbers\":[7,2,5,1,3]}", 3)]
    [TestCase("=median($.numbers[*])", "{\"numbers\":[4,2,6,8]}", 5)]
    [TestCase("=median($.value)", "{\"value\":42}", 42)]
    [TestCase("=median($.a,$.b,$.c)", "{\"a\":10,\"b\":5,\"c\":15}", 10)]
    public void MedianTests_ValidInputs(string function, string data, double resultValue)
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
    public void Median_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(MedianBuilders.Median("1", "3", "5", "7", "9"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Median_HandlesEvenNumberOfElements()
    {
        var script = new JLioScript()
                .Add(MedianBuilders.Median("2", "4", "6", "8"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Median_WorksWithComplexArrays()
    {
        var data = "{\"students\":[{\"grade\":85},{\"grade\":92},{\"grade\":78},{\"grade\":95},{\"grade\":88}]}";
        var script = "[{\"path\":\"$.medianGrade\",\"value\":\"=median($.students[*].grade)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(88, result.Data.SelectToken("$.medianGrade")?.Value<double>());
    }

    [Test]
    public void Median_FailsWithNoArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=median()\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}