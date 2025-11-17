using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class ConditionalAggregateFunctionsTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText();
        executionContext = ExecutionContext.CreateDefault();
    }

    #region SUMIF Tests

    [Test]
    public void SumIf_NumericGreaterThan()
    {
        var script = "[{'path':'$.result','value':'=sumif($.numbers[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"numbers\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(16, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_NumericGreaterThanOrEqual()
    {
        var script = "[{'path':'$.result','value':'=sumif($.numbers[*],\\'>=5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"numbers\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(21, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_NumericLessThan()
    {
        var script = "[{'path':'$.result','value':'=sumif($.numbers[*],\\'<5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"numbers\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_WithSumRange()
    {
        var script = "[{'path':'$.result','value':'=sumif($.items[*].category,\\'electronics\\',$.items[*].price)','command':'add'}]".Replace("'", "\"");
        var data = "{\"items\":[{\"category\":\"electronics\",\"price\":100},{\"category\":\"books\",\"price\":20},{\"category\":\"electronics\",\"price\":150}]}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(250, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_Wildcards()
    {
        var script = "[{'path':'$.result','value':'=sumif($.names[*],\\'*john*\\',$.values[*])','command':'add'}]".Replace("'", "\"");
        var data = "{\"names\":[\"john\",\"jane\",\"johnny\",\"bob\"],\"values\":[10,20,30,40]}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(40, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_IgnoresNonNumericValues()
    {
        var script = "[{'path':'$.result','value':'=sumif($.values[*],\\'>0\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[1,\"text\",3,null,5]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(9, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIf_NoMatchesReturnsZero()
    {
        var script = "[{'path':'$.result','value':'=sumif($.values[*],\\'>100\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[1,2,3]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    #endregion

    #region SUMIFS Tests

    [Test]
    public void SumIfs_SingleCriteria()
    {
        var script = "[{'path':'$.result','value':'=sumifs($.values[*],$.values[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(16, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIfs_MultipleCriteria()
    {
        var script = "[{'path':'$.result','value':'=sumifs($.prices[*],$.prices[*],\\'>=10\\',$.prices[*],\\'<=50\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"prices\":[5,15,25,55,30]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(70, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void SumIfs_DifferentRanges()
    {
        var script = "[{'path':'$.result','value':'=sumifs($.sales[*],$.categories[*],\\'electronics\\',$.status[*],\\'active\\')','command':'add'}]".Replace("'", "\"");
        var data = "{\"sales\":[100,50,150,75],\"categories\":[\"electronics\",\"books\",\"electronics\",\"books\"],\"status\":[\"active\",\"active\",\"inactive\",\"active\"]}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(100, result.Data.SelectToken("$.result")?.Value<double>());
    }

    #endregion

    #region COUNTIF Tests

    [Test]
    public void CountIf_NumericComparison()
    {
        var script = "[{'path':'$.result','value':'=countif($.numbers[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"numbers\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void CountIf_TextEquality()
    {
        var script = "[{'path':'$.result','value':'=countif($.items[*],\\'electronics\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"items\":[\"electronics\",\"books\",\"electronics\",\"music\"]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void CountIf_Wildcards()
    {
        var script = "[{'path':'$.result','value':'=countif($.names[*],\\'*john*\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"names\":[\"john\",\"jane\",\"johnny\",\"bob\"]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")?.Value<int>());
    }

    #endregion

    #region COUNTIFS Tests

    [Test]
    public void CountIfs_SingleCriteria()
    {
        var script = "[{'path':'$.result','value':'=countifs($.values[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void CountIfs_MultipleCriteria()
    {
        var script = "[{'path':'$.result','value':'=countifs($.values[*],\\'>=5\\',$.values[*],\\'<=10\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2,12]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<int>());
    }

    [Test]
    public void CountIfs_DifferentRanges()
    {
        var script = "[{'path':'$.result','value':'=countifs($.categories[*],\\'electronics\\',$.status[*],\\'active\\')','command':'add'}]".Replace("'", "\"");
        var data = "{\"categories\":[\"electronics\",\"books\",\"electronics\",\"books\"],\"status\":[\"active\",\"active\",\"inactive\",\"active\"]}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.SelectToken("$.result")?.Value<int>());
    }

    #endregion

    #region AVERAGEIF Tests

    [Test]
    public void AverageIf_NumericComparison()
    {
        var script = "[{'path':'$.result','value':'=averageif($.numbers[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"numbers\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(8, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void AverageIf_WithAverageRange()
    {
        var script = "[{'path':'$.result','value':'=averageif($.items[*].category,\\'electronics\\',$.items[*].price)','command':'add'}]".Replace("'", "\"");
        var data = "{\"items\":[{\"category\":\"electronics\",\"price\":100},{\"category\":\"books\",\"price\":20},{\"category\":\"electronics\",\"price\":200}]}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(150, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void AverageIf_IgnoresNonNumericValues()
    {
        var script = "[{'path':'$.result','value':'=averageif($.values[*],\\'>0\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[2,\"text\",4,null,6]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(4, result.Data.SelectToken("$.result")?.Value<double>());
    }

    #endregion

    #region AVERAGEIFS Tests

    [Test]
    public void AverageIfs_SingleCriteria()
    {
        var script = "[{'path':'$.result','value':'=averageifs($.values[*],$.values[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(8, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void AverageIfs_MultipleCriteria()
    {
        var script = "[{'path':'$.result','value':'=averageifs($.values[*],$.values[*],\\'>=5\\',$.values[*],\\'<=10\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2,12]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(7, result.Data.SelectToken("$.result")?.Value<double>());
    }

    #endregion

    #region MINIFS Tests

    [Test]
    public void MinIfs_SingleCriteria()
    {
        var script = "[{'path':'$.result','value':'=minifs($.values[*],$.values[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(7, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void MinIfs_MultipleCriteria()
    {
        var script = "[{'path':'$.result','value':'=minifs($.values[*],$.values[*],\\'>=5\\',$.values[*],\\'<=10\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2,12]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void MinIfs_NoMatchesReturnsError()
    {
        var script = "[{'path':'$.result','value':'=minifs($.values[*],$.values[*],\\'>100\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[1,2,3]}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    #endregion

    #region MAXIFS Tests

    [Test]
    public void MaxIfs_SingleCriteria()
    {
        var script = "[{'path':'$.result','value':'=maxifs($.values[*],$.values[*],\\'>5\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(9, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void MaxIfs_MultipleCriteria()
    {
        var script = "[{'path':'$.result','value':'=maxifs($.values[*],$.values[*],\\'>=5\\',$.values[*],\\'<=10\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[3,5,7,9,2,12]}"), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(9, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void MaxIfs_NoMatchesReturnsError()
    {
        var script = "[{'path':'$.result','value':'=maxifs($.values[*],$.values[*],\\'>100\\')','command':'add'}]".Replace("'", "\"");
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{\"values\":[1,2,3]}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    #endregion
}
