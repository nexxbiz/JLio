using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class SumTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executionContext = ExecutionContext.CreateDefault();
    }

    [TestCase("=sum()", "{}", 0)]
    [TestCase("=sum(1,2,3)", "{}", 6)]
    [TestCase("=sum($.a,$.b,$.c)", "{\"a\":1,\"b\":2,\"c\":3}", 6)]
    [TestCase("=sum()", "{}", 0)]
    [TestCase("=sum(1,2,3)", "{}", 6)]
    [TestCase("=sum($.a,$.b,$.c)", "{\"a\":1,\"b\":2,\"c\":3}", 6)]
    // Child object test cases
    [TestCase("=sum($.user.age,$.user.score)", "{\"user\":{\"age\":25,\"score\":100}}", 125)]
    [TestCase("=sum($.customer.billing.amount,$.customer.shipping.cost)", "{\"customer\":{\"billing\":{\"amount\":50.5},\"shipping\":{\"cost\":9.95}}}", 60.45)]
    [TestCase("=sum($.config.settings.timeout,$.config.settings.retries,$.config.settings.maxConnections)", "{\"config\":{\"settings\":{\"timeout\":30,\"retries\":3,\"maxConnections\":10}}}", 43)]
    // Array of numbers test cases
    [TestCase("=sum($.numbers[0],$.numbers[1],$.numbers[2])", "{\"numbers\":[10,20,30]}", 60)]
    [TestCase("=sum($.scores[*])", "{\"scores\":[85,92,78,96]}", 351)]
    [TestCase("=sum($.values[0],$.values[2])", "{\"values\":[5,15,25,35]}", 30)]
    // Array of objects test cases
    [TestCase("=sum($.products[0].price,$.products[1].price)", "{\"products\":[{\"name\":\"A\",\"price\":19.99},{\"name\":\"B\",\"price\":29.99}]}", 49.98)]
    [TestCase("=sum($.employees[*].salary)", "{\"employees\":[{\"name\":\"John\",\"salary\":50000},{\"name\":\"Jane\",\"salary\":60000},{\"name\":\"Bob\",\"salary\":45000}]}", 155000)]
    [TestCase("=sum($.orders[*].total)", "{\"orders\":[{\"id\":1,\"total\":125.50},{\"id\":2,\"total\":75.25},{\"id\":3,\"total\":200.00}]}", 400.75)]
    // Mixed nested structures
    [TestCase("=sum($.company.departments[0].budget,$.company.departments[1].budget)", "{\"company\":{\"departments\":[{\"name\":\"IT\",\"budget\":100000},{\"name\":\"HR\",\"budget\":50000}]}}", 150000)]
    [TestCase("=sum($.data.metrics.cpu,$.data.metrics.memory,$.data.alerts[*].priority)", "{\"data\":{\"metrics\":{\"cpu\":75,\"memory\":60},\"alerts\":[{\"type\":\"warning\",\"priority\":2},{\"type\":\"error\",\"priority\":5}]}}", 142)]
    [TestCase("=sum($.inventory[*].items[0].quantity)", "{\"inventory\":[{\"category\":\"electronics\",\"items\":[{\"name\":\"laptop\",\"quantity\":5}]},{\"category\":\"books\",\"items\":[{\"name\":\"novel\",\"quantity\":12}]}]}", 17)]
    // Complex nested array structures
    [TestCase("=sum($.regions[*].stores[*].revenue)", "{\"regions\":[{\"name\":\"North\",\"stores\":[{\"id\":1,\"revenue\":10000},{\"id\":2,\"revenue\":15000}]},{\"name\":\"South\",\"stores\":[{\"id\":3,\"revenue\":12000},{\"id\":4,\"revenue\":8000}]}]}", 45000)]
    [TestCase("=sum($.categories[0].subcategories[*].count)", "{\"categories\":[{\"name\":\"tech\",\"subcategories\":[{\"name\":\"phones\",\"count\":25},{\"name\":\"tablets\",\"count\":15}]},{\"name\":\"books\",\"subcategories\":[{\"name\":\"fiction\",\"count\":100}]}]}", 40)]
    public void sumTests(string function, string data, double resultValue)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.IsNotNull(result.Data.SelectToken("$.result"));
        Assert.AreEqual(resultValue, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void ScriptFailsOnInvalidValue()
    {
        var script = "[{'path':'$.result','value':'=sum($.a)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"a\":\"x\"}"), executionContext);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(SumBuilders.Sum("1", "2"))
                .OnPath("$.result");
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<double>());
    }
}
