using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Text;
using JLio.Extensions.Text.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class LengthTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=length('hello')", "{}", 5)]
    [TestCase("=length('')", "{}", 0)]
    [TestCase("=length('hello world')", "{}", 11)]
    [TestCase("=length($.text)", "{\"text\":\"JLio\"}", 4)]
    [TestCase("=length($.text)", "{\"text\":\"\"}", 0)]
    [TestCase("=length($.missing)", "{}", 0)] // Testing missing property (null)
    public void LengthTests_ValidInputs(string function, string data, int expectedLength)
    {
        var script = $"{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}";
        var scriptArray = $"[{script}]";
        var result = JLioConvert.Parse(scriptArray, parseOptions).Execute(JToken.Parse(data), executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(expectedLength, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void Length_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(LengthBuilders.Length("$.name"))
                .OnPath("$.nameLength");
        var result = script.Execute(JToken.Parse("{\"name\":\"Testing\"}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(7, result.Data.SelectToken("$.nameLength")?.Value<int>());
    }

    [Test]
    public void Length_FailsWithMultipleArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=length('a','b')\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}