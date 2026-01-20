using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Extensions.Math.Builders;
using JLio.Functions.Builders;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class NewBuildersTests
{
    [Test]
    public void ScriptPathBuilder_CreatesScriptPathFunction()
    {
        var script = new JLioScript()
            .Add(ScriptPathBuilders.ScriptPath("$.someValue"))
            .OnPath("$.result");
        Assert.IsNotNull(script);
    }

    [Test]
    public void SumIfBuilder_CreatesSumIfFunction()
    {
        var script = new JLioScript()
            .Add(SumIfBuilders.SumIf("$.numbers[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"numbers\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(16, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void CountIfBuilder_CreatesCountIfFunction()
    {
        var script = new JLioScript()
            .Add(CountIfBuilders.CountIf("$.numbers[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"numbers\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void AverageIfBuilder_CreatesAverageIfFunction()
    {
        var script = new JLioScript()
            .Add(AverageIfBuilders.AverageIf("$.numbers[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"numbers\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(8, result.Data.SelectToken("$.result")!.Value<double>());
    }

    [Test]
    public void SumIfsBuilder_CreatesSumIfsFunction()
    {
        var script = new JLioScript()
            .Add(SumIfsBuilders.SumIfs("$.values[*]", "$.values[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"values\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(16, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void CountIfsBuilder_CreatesCountIfsFunction()
    {
        var script = new JLioScript()
            .Add(CountIfsBuilders.CountIfs("$.values[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"values\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void AverageIfsBuilder_CreatesAverageIfsFunction()
    {
        var script = new JLioScript()
            .Add(AverageIfsBuilders.AverageIfs("$.values[*]", "$.values[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"values\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(8, result.Data.SelectToken("$.result")!.Value<double>());
    }

    [Test]
    public void MaxIfsBuilder_CreatesMaxIfsFunction()
    {
        var script = new JLioScript()
            .Add(MaxIfsBuilders.MaxIfs("$.values[*]", "$.values[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"values\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(9, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void MinIfsBuilder_CreatesMinIfsFunction()
    {
        var script = new JLioScript()
            .Add(MinIfsBuilders.MinIfs("$.values[*]", "$.values[*]", "'>5'"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"values\":[3,5,7,9,2]}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(7, result.Data.SelectToken("$.result")!.Value<int>());
    }


}
