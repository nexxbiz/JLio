using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class FetchTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void FetchValueByPath()
    {
        var script = "[{'path':'$.result','value':'=fetch($.source)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse("{ 'source': 5 }".Replace("'","\"")), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void FetchReturnsNullWhenPathMissing()
    {
        var script = "[{'path':'$.result','value':'=fetch($.missing)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchWithNoArgumentsReturnsNull()
    {
        var script = "[{'path':'$.result','value':'=fetch()','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }
}
