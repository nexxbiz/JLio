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

namespace JLio.UnitTests.FunctionsTests;

public class CountTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=count()", "{}", 0)]
    [TestCase("=count(1,2,3)", "{}", 3)]
    [TestCase("=count($.a,$.b)", "{\"a\":1,\"b\":2}", 2)]
    [TestCase("=count($.numbers)", "{\"numbers\":[1,2,3]}", 3)]
    [TestCase("=count($.obj)", "{\"obj\":{\"a\":1,\"b\":2}}", 1)]
    public void countTests(string function, string data, int resultValue)
    {
        var script = $"{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}";
        var scriptArray = $"[{script}]";
        var result = JLioConvert.Parse(scriptArray, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void Count_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(CountBuilders.Count("$.items"))
                .OnPath("$.count");
        var token = JToken.Parse("{\"items\":[1,2,3]}");
        var result = script.Execute(token);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.count")?.Value<int>());
    }
}
