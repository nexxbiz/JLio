using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Functions.Builders;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class FetchBuildersTests
{
    [Test]
    public void BuilderCreatesFetchFunction()
    {
        var script = new JLioScript()
            .Add(FetchBuilders.Fetch("$.value"))
            .OnPath("$.result");
        var result = script.Execute(JObject.Parse("{\"value\":5}"));
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")!.Value<int>());
    }
}
