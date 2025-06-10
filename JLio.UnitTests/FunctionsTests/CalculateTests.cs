using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Math.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class CalculateExtendedTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath();
        executionContext = ExecutionContext.CreateDefault();
    }

    #region Basic Arithmetic Tests
    [TestCase("=calculate('2+3')", "{}", 5)]
    [TestCase("=calculate('10-7')", "{}", 3)]
    [TestCase("=calculate('6*4')", "{}", 24)]
    [TestCase("=calculate('15/3')", "{}", 5)]
    [TestCase("=calculate('17%5')", "{}", 2)]
    public void BasicArithmeticTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion

    #region Parentheses and Order of Operations Tests
    [TestCase("=calculate('(2+3)*4')", "{}", 20)]
    [TestCase("=calculate('2+(3*4)')", "{}", 14)]
    [TestCase("=calculate('((2+3)*4)+1')", "{}", 21)]
    [TestCase("=calculate('(10-6)/(2*2)')", "{}", 1)]
    [TestCase("=calculate('(5+3)*(2-1)')", "{}", 8)]
    [TestCase("=calculate('(2*(3+4))-(5*2)')", "{}", 4)]
    [TestCase("=calculate('((2+3)*(4+1))-((3*2)+4)')", "{}", 15)]
    public void ParenthesesTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>(), 0.0001);
    }
    #endregion

    #region Decimal Tests (with period notation)
    [TestCase("=calculate('2.5+3.7')", "{}", 6.2)]
    [TestCase("=calculate('10.5-7.25')", "{}", 3.25)]
    [TestCase("=calculate('3.14*2')", "{}", 6.28)]
    [TestCase("=calculate('15.75/3.15')", "{}", 5)]
    [TestCase("=calculate('(2.5+1.5)*3.2')", "{}", 12.8)]
    [TestCase("=calculate('0.1+0.2')", "{}", 0.3)]
    [TestCase("=calculate('1.234567*2')", "{}", 2.469134)]
    public void DecimalWithPeriodTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>(), 0.000001);
    }
    #endregion

    #region Decimal Tests (with comma notation - European style)
    [TestCase("=calculate('2,5+3,7')", "{}", 6.2)]
    [TestCase("=calculate('10,5-7,25')", "{}", 3.25)]
    [TestCase("=calculate('3,14*2')", "{}", 6.28)]
    [TestCase("=calculate('15,75/3,15')", "{}", 5)]
    [TestCase("=calculate('(2,5+1,5)*3,2')", "{}", 12.8)]
    [TestCase("=calculate('0,1+0,2')", "{}", 0.3)]
    public void DecimalWithCommaTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>(), 0.000001);
    }
    #endregion

    #region Simple Variable Substitution Tests
    [TestCase("=calculate('2+{{$.v}}')", "{\"v\":3}", 5)]
    [TestCase("=calculate('{{$.a}}*{{$.b}}')", "{\"a\":4,\"b\":7}", 28)]
    [TestCase("=calculate('{{$.x}}-{{$.y}}')", "{\"x\":15,\"y\":8}", 7)]
    [TestCase("=calculate('{{$.dividend}}/{{$.divisor}}')", "{\"dividend\":20,\"divisor\":4}", 5)]
    public void SimpleVariableTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion

    #region Nested Object Tests
    [TestCase("=calculate('{{$.user.age}}+5')", "{\"user\":{\"age\":25}}", 30)]
    [TestCase("=calculate('{{$.account.balance}}*{{$.account.interestRate}}')", "{\"account\":{\"balance\":1000,\"interestRate\":0.05}}", 50)]
    [TestCase("=calculate('{{$.order.items.price}}+{{$.order.tax}}')", "{\"order\":{\"items\":{\"price\":99.99},\"tax\":8.50}}", 108.49)]
    [TestCase("=calculate('{{$.data.measurements.length}}*{{$.data.measurements.width}}')", "{\"data\":{\"measurements\":{\"length\":12.5,\"width\":8.3}}}", 103.75)]
    [TestCase("=calculate('({{$.person.salary}}/12)*{{$.person.taxRate}}')", "{\"person\":{\"salary\":60000,\"taxRate\":0.25}}", 1250)]
    public void NestedObjectTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>(), 0.01);
    }
    #endregion

    #region Array Index Tests
    [TestCase("=calculate('{{$.numbers[0]}}+{{$.numbers[1]}}')", "{\"numbers\":[10,15,20]}", 25)]
    [TestCase("=calculate('{{$.prices[2]}}*{{$.quantities[2]}}')", "{\"prices\":[5.99,7.50,12.25],\"quantities\":[2,3,4]}", 49)]
    [TestCase("=calculate('(({{$.scores[0]}}+{{$.scores[1]}}+{{$.scores[2]}})/3)')", "{\"scores\":[84,92,79]}", 85)]
    [TestCase("=calculate('{{$.matrix[0][1]}}*{{$.matrix[1][0]}}')", "{\"matrix\":[[1,2,3],[4,5,6]]}", 8)]
    public void ArrayIndexTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion

    #region Complex Nested Retrieval Tests
    [TestCase("=calculate('{{$.company.departments[0].budget}}+{{$.company.departments[1].budget}}')",
              "{\"company\":{\"departments\":[{\"name\":\"IT\",\"budget\":50000},{\"name\":\"HR\",\"budget\":30000}]}}",
              80000)]
    [TestCase("=calculate('{{$.invoice.items[0].quantity}}*{{$.invoice.items[0].unitPrice}}')",
              "{\"invoice\":{\"items\":[{\"name\":\"Widget\",\"quantity\":5,\"unitPrice\":12.99}]}}",
              64.95)]
    [TestCase("=calculate('{{$.students[2].grades.math}}+{{$.students[2].grades.science}}')",
              "{\"students\":[{},{},{\"name\":\"John\",\"grades\":{\"math\":85,\"science\":92}}]}",
              177)]
    [TestCase("=calculate('{{$.config.server.cpu.cores}}*{{$.config.server.memory.gbPerCore}}')",
              "{\"config\":{\"server\":{\"cpu\":{\"cores\":8},\"memory\":{\"gbPerCore\":4}}}}",
              32)]
    public void ComplexNestedRetrievalTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>(), 0.01);
    }
    #endregion

    #region Mixed Complex Operations Tests
    [TestCase("=calculate('({{$.a}}+{{$.b.nested}})*({{$.c[0]}}-{{$.d.deep.value}})')",
              "{\"a\":5,\"b\":{\"nested\":3},\"c\":[10,20],\"d\":{\"deep\":{\"value\":2}}}",
              64)] // (5+3)*(10-2) = 8*8 = 64
    [TestCase("=calculate('{{$.order.subtotal}}+({{$.order.subtotal}}*{{$.tax.rate}})')",
              "{\"order\":{\"subtotal\":100},\"tax\":{\"rate\":0.08}}",
              108)]
    public void MixedComplexOperationsTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion

    #region Negative Numbers and Edge Cases
    [TestCase("=calculate('-5+10')", "{}", 5)]
    [TestCase("=calculate('(-3)*4')", "{}", -12)]
    [TestCase("=calculate('10/(-2)')", "{}", -5)]
    [TestCase("=calculate('{{$.negative}}*(-1)')", "{\"negative\":-7}", 7)]
    [TestCase("=calculate('0*{{$.anyValue}}')", "{\"anyValue\":999}", 0)]
    public void NegativeNumbersAndEdgeCasesTests(string function, string data, double expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error));
        Assert.AreEqual(expected, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion

    #region Error Cases Tests
    [Test]
    public void CalculateFailsOnMultiToken()
    {
        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('1+{{ $.arr[*] }}')"",
            ""command"": ""add""
        }
    ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{\"arr\":[1,2]}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CalculateFailsOnDivisionByZero()
    {
        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('10/0')"",
            ""command"": ""add""
        }
    ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CalculateFailsOnMissingProperty()
    {
        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('{{$.nonExistent}}+5')"",
            ""command"": ""add""
        }
    ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void CalculateFailsOnInvalidExpression()
    {
        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('2++')"",
            ""command"": ""add""
        }
    ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse("{}"), executionContext);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
    #endregion

    #region Fluent API Tests
    [Test]
    public void CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
                .Add(CalculateBuilders.Calculate("1+{{$.v}}"))
                .OnPath("$.result");
        var token = JToken.Parse("{\"v\":2}");
        var result = script.Execute(token);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void FluentApiWithComplexExpression()
    {
        var script = new JLioScript()
                .Add(CalculateBuilders.Calculate("({{$.a}}+{{$.b}})*{{$.multiplier}}"))
                .OnPath("$.calculation");
        var token = JToken.Parse("{\"a\":10,\"b\":5,\"multiplier\":3}");
        var result = script.Execute(token);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(45, result.Data.SelectToken("$.calculation")?.Value<double>());
    }

    [Test]
    public void FluentApiWithNestedData()
    {
        var script = new JLioScript()
                .Add(CalculateBuilders.Calculate("{{$.product.price}}*{{$.order.quantity}}"))
                .OnPath("$.total");
        var token = JToken.Parse("{\"product\":{\"price\":25.99},\"order\":{\"quantity\":3}}");
        var result = script.Execute(token);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(77.97, result.Data.SelectToken("$.total")?.Value<double>(), 0.01);
    }
    #endregion

    #region Performance and Stress Tests
    [Test]
    public void CalculateWithDeeplyNestedStructure()
    {
        var complexData = @"{
            ""level1"": {
                ""level2"": {
                    ""level3"": {
                        ""level4"": {
                            ""level5"": {
                                ""value"": 42
                            }
                        }
                    }
                }
            }
        }";

        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('{{$.level1.level2.level3.level4.level5.value}}*2')"",
            ""command"": ""add""
        }
    ]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse(complexData), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(84, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void CalculateWithLargeNumbers()
    {
        var script = @"[
        {
            ""path"": ""$.result"",
            ""value"": ""=calculate('{{$.large1}}+{{$.large2}}')"",
            ""command"": ""add""
        }
    ]";
        var data = "{\"large1\":999999999,\"large2\":1000000001}";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2000000000, result.Data.SelectToken("$.result")?.Value<double>());
    }
    #endregion
}