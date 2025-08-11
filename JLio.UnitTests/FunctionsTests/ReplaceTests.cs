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

public class ReplaceTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=replace('hello world', 'world', 'JLio')", "{}", "hello JLio")]
    [TestCase("=replace('Hello World', 'WORLD', 'JLio', true)", "{}", "Hello JLio")]  // Case insensitive
    [TestCase("=replace('Hello World', 'WORLD', 'JLio', false)", "{}", "Hello World")] // Case sensitive
    [TestCase("=replace('test test test', 'test', 'demo')", "{}", "demo demo demo")]
    [TestCase("=replace($.text, 'old', 'new')", "{\"text\":\"old value\"}", "new value")]
    [TestCase("=replace('', 'a', 'b')", "{}", "")]
    public void ReplaceTests_ValidInputs(string function, string data, string expected)
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
    public void Replace_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(ReplaceBuilders.Replace("$.message", "Hello", "Hi"))
                .OnPath("$.modified");
        var result = script.Execute(JToken.Parse("{\"message\":\"Hello World\"}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hi World", result.Data.SelectToken("$.modified")?.Value<string>());
    }

    [Test]
    public void Replace_FailsWithEmptyOldValue()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=replace('test', '', 'new')\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
}