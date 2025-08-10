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

public class FormatTests
{
    private IExecutionContext executeContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterText();
        executeContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=format($.source,'dd-MM-yyyy')", "{\"source\" : \"2022-01-10T21:15:15.113Z\"}",
        "\"10-01-2022\"")]
    [TestCase("=format($.source,'dd-MM-yyyy')", "{\"source\" : \"11/29/2023 1:54:49 PM\"}",
        "\"29-11-2023\"")]
    public void Format_ScriptTestWithPath(string function, string data, string resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(resultValue), result.Data.SelectToken("$.result")));
    }

    [TestCase("=format('dd-MM-yyyy')", "{\"source\" : \"2022-01-10T21:15:15.113Z\"}",
        "\"10-01-2022\"")]
    public void ScriptTestWithoutPath(string function, string data, string expectedResult)
    {
        var script = $"[{{\"path\":\"$.source\",\"value\":\"{function}\",\"command\":\"set\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.source")));
    }

    [Test]
    public void CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Set(FormatBuilders.FormatPath("$.demo2").UsingFormat("dd-MM"))
                .OnPath("$.id")
                .Set(FormatBuilders.Format("dd-MM-yyyy"))
                .OnPath("$.demo")
            ;
        var result =
            script.Execute(
                JToken.Parse(
                    "{\"demo\" : \"2022-01-10T21:15:15.113Z\", \"demo2\" : \"2022-01-10T21:15:15.113Z\", \"id\" : 4 }"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }
    [Test]
    public void FormatHandlesDateToken()
    {
        var token = new JObject { ["source"] = new JValue(new System.DateTime(2024,1,1,8,0,0, System.DateTimeKind.Utc)) };
        var function = "=format($.source,'yyyy-MM-dd')";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(token, executeContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse("\"2024-01-01\""), result.Data.SelectToken("$.result")));
    }

    [Test]
    public void FormatWithInvalidArgumentsFails()
    {
        var script = "[{'path':'$.result','value':'=format()','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executeContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void FormatLeavesNonDateStringUnchanged()
    {
        var token = JObject.Parse("{\"source\":\"not a date\"}");
        var function = "=format($.source,'dd-MM-yyyy')";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(token, executeContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("not a date", result.Data.SelectToken("$.result")?.Value<string>());
    }

    [Test]
    public void FormatLeavesNumericValueUnchanged()
    {
        var token = JObject.Parse("{\"source\":123}");
        var function = "=format($.source,'dd-MM-yyyy')";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(token, executeContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(123, result.Data.SelectToken("$.result")?.Value<int>());
    }
}
