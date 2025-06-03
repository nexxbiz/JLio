using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class FormatTests
{
    private IExecutionContext executeContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executeContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=format($.source,'dd-MM-yyyy')", "{\"source\" : \"2022-01-10T21:15:15.113Z\"}",
        "\"10-01-2022\"")]
    [TestCase("=format($.source,'dd-MM-yyyy')", "{\"source\" : \"11/29/2023 1:54:49 PM\"}",
        "\"29-11-2023\"")]
    public void ScriptTestWithPath(string function, string data, string expectedResult)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
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
                .Set(new Format("$.demo2", "dd-MM"))
                .OnPath("$.id")
                .Set(new Format("dd-MM-yyyy"))
                .OnPath("$.demo")
            ;
        var result =
            script.Execute(
                JToken.Parse(
                    "{\"demo\" : \"2022-01-10T21:15:15.113Z\", \"demo2\" : \"2022-01-10T21:15:15.113Z\", \"id\" : 4 }"));

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }
}