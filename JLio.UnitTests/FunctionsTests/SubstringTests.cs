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

public class SubstringTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=substring('hello world', 0, 5)", "{}", "hello")]
    [TestCase("=substring('hello world', 6)", "{}", "world")]
    [TestCase("=substring('hello', 1, 3)", "{}", "ell")]
    [TestCase("=substring('test', 2)", "{}", "st")]  // Fixed: test[2:] = "st"
    [TestCase("=substring($.text, 0, 3)", "{\"text\":\"JLio\"}", "JLi")]
    [TestCase("=substring('hello', 0)", "{}", "hello")]
    public void SubstringTests_ValidInputs(string function, string data, string expected)
    {
        var script = $"{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}";
        var scriptArray = $"[{script}]";
        var result = JLioConvert.Parse(scriptArray, parseOptions).Execute(JToken.Parse(data), executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void Substring_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(SubstringBuilders.Substring("$.message", "7", "5"))
                .OnPath("$.extracted");
        var result = script.Execute(JToken.Parse("{\"message\":\"Hello, World!\"}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("World", result.Data.SelectToken("$.extracted")?.Value<string>());
    }

    [Test]
    public void Substring_FailsWithInvalidIndex()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=substring('test', 10)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}