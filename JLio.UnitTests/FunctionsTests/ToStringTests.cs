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

public class ToStringTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=toString()", "{\"result\" : 3 }", "3")]
    [TestCase("=toString()", "{\"result\" : \"3\"}", "3")]
    [TestCase("=toString()", "{\"result\" : {\"demo\":67}}", "{\"demo\":67}")]
    public void ScriptTestSet(string function, string data, string expectedResult)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(expectedResult, result.Data.SelectToken("$.result")?.ToString());
    }

    [TestCase("=toString($.item)", "{\"item\" : 3 }", "3")]
    [TestCase("=toString($.item)", "{\"item\" : \"3\"}", "3")]
    [TestCase("=toString($.item)", "{\"item\" : {\"demo\":67}}", "{\r\n  \"demo\": 67\r\n}")]
    public void ScriptTestAdd(string function, string data, string expectedResult)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult),
            JToken.Parse(result.Data.SelectToken("$.result").ToString())));
    }

    [Test]
    public void CanbeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Set(ToStringBuilders.ToString())
                .OnPath("$.id")
                .Set(ToStringBuilders.ToString("$.result"))
                .OnPath("$.resultDemo")
            ;
        var result = script.Execute(JObject.Parse("{\"result\" : 3 }"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreNotEqual(result.Data.SelectToken("$.result")?.Type, JTokenType.Null);
        Assert.AreEqual(result.Data.SelectToken("$.result")?.ToString(), "3");
    }
}