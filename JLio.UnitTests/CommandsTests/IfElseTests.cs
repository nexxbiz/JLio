using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class IfElseTests
{
    private IExecutionContext executeOptions;
    private FunctionConverter functionConverter;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        functionConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
    }

    private IFunctionSupportedValue Val(JToken v) => new FunctionSupportedValue(new FixedValue(v, functionConverter));

    [Test]
    public void ExecutesIfBranchWhenConditionMatches()
    {
        var data = JObject.Parse("{\"aaa\":{\"bb\":[{\"revenue\":200}]},\"result\":0}");
        var ifScript = new JLioScript()
            .Set(new JValue(1))
            .OnPath("$.result");
        var elseScript = new JLioScript()
            .Set(new JValue(-1))
            .OnPath("$.result");

        var command = new IfElse
        {
            First = Val(new JValue("$.aaa.bb[0].revenue")),
            Second = Val(new JValue(200)),
            IfScript = ifScript,
            ElseScript = elseScript
        };

        var script = new JLioScript();
        script.AddLine(command);

        var result = script.Execute(data, executeOptions);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void ExecutesElseBranchWhenConditionDoesNotMatch()
    {
        var data = JObject.Parse("{\"aaa\":{\"bb\":[{\"revenue\":100}]},\"result\":0}");
        var ifScript = new JLioScript()
            .Set(new JValue(1))
            .OnPath("$.result");
        var elseScript = new JLioScript()
            .Set(new JValue(-1))
            .OnPath("$.result");

        var command = new IfElse
        {
            First = Val(new JValue("$.aaa.bb[0].revenue")),
            Second = Val(new JValue(200)),
            IfScript = ifScript,
            ElseScript = elseScript
        };

        var script = new JLioScript();
        script.AddLine(command);

        var result = script.Execute(data, executeOptions);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(-1, data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void CanParseWithNestedScripts()
    {
        var scriptText = "[{\"first\":\"$.value\",\"second\":1,\"ifScript\":[{\"path\":\"$.result\",\"value\":\"if\",\"command\":\"add\"}],\"elseScript\":[{\"path\":\"$.result\",\"value\":\"else\",\"command\":\"add\"}],\"command\":\"ifElse\"}]";

        var script = JLioConvert.Parse(scriptText);

        var data = JObject.Parse("{\"value\":1}");
        var result = script.Execute(data, executeOptions);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("if", data.SelectToken("$.result")?.ToString());
    }
}
