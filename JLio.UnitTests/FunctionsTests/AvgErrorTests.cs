using JLio.Client;
using JLio.Core.Models;
using JLio.Extensions.Math;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class AvgErrorTests
{
    [Test]
    public void AvgLogsErrorForInvalidType()
    {
        var options = ParseOptions.CreateDefault().RegisterMath();
        var context = ExecutionContext.CreateDefault();
        var script = "[{'path':'$.result','value':'=avg($.obj)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, options).Execute(JObject.Parse("{ 'obj': { 'a': 1 } }".Replace("'","\"")), context);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(context.GetLogEntries().Any(e => e.Message.Contains("can only handle numeric values")));
    }
}
