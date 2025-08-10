using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Text;
using JLio.Extensions.Text.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class ConcatTests
{
    private IExecutionContext executeOptions;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterText();
        executeOptions = ExecutionContext.CreateDefault();
    }

    [TestCase("=concat()", "{}", "")]
    [TestCase("=concat('a','b','c')", "{}", "abc")]
    [TestCase("= concat ( 'a' , concat('a','b','c') ,  'concat('a','b','c')' )", "{}", "aabcconcat('a','b','c')")]
    [TestCase("=concat($.a, 'b', $.c)", "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
    [TestCase("=concat($.a, $.b, $.c)",
        "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
    public void ScriptTest(string function, string data, string resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.ToString());
    }

    [TestCase("=concat($.a, $.b, $.c)555",
        "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
    public void ScriptTestWithWarnings(string function, string data, string resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.ToString());
    }

    [Test]
    public void Concat_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(ConcatBuilders.Concat("'a'", "'b'"))
                .OnPath("$.result")
            ;
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("ab", result.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void Concat_CanBeUsedInFluentApi_Set()
    {
        var script = new JLioScript()
            .Set(ConcatBuilders.Concat("'a'", "'b'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"result\":\"something\"}"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("ab", result.Data.SelectToken("$.result")?.Value<string>());
    }

    [TestCase("=concat()", "{}", "")]
    [TestCase("=concat('a','b','c')", "{}", "abc")]
    [TestCase("= concat ( 'a' , concat('a','b','c') ,  'concat('a','b','c')' )", "{}", "aabcconcat('a','b','c')")]
    [TestCase("=concat($.a, 'b', $.c)", "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
    [TestCase("=concat($.a, $.b, $.c)",
        "{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}", "abc")]
    public void Concat_ScriptTest(string function, string data, string resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<string>());
    }
}