using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JLio.UnitTests.ExtensionsTests.MathTesting;

[TestFixture]
public class CalculatePerformanceTests
{
    private IExecutionContext executeOptions;
    private JToken largeDataset;
    private Calculate function;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        largeDataset = GenerateDataset(10000);
        function = new Calculate();
    }

    private JToken GenerateDataset(int size)
    {
        var calculations = new JArray();
        var random = new Random(42);
        for (int i = 0; i < size; i++)
        {
            calculations.Add(new JObject
            {
                ["id"] = i,
                ["valueA"] = Math.Round(random.NextDouble() * 1000, 2),
                ["valueB"] = Math.Round(random.NextDouble() * 100 + 1, 2),
                ["valueC"] = Math.Round(random.NextDouble() * 50, 2),
                ["percentage"] = Math.Round(random.NextDouble() * 100, 2),
                ["multiplier"] = Math.Round(random.NextDouble() * 10 + 1, 2)
            });
        }
        return new JObject { ["calculations"] = calculations };
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_SimpleArithmetic_10000_Records()
    {
        var expressions = new[] { "{{@.valueA}} + {{@.valueB}}", "{{@.valueA}} - {{@.valueB}}", "{{@.valueA}} * {{@.valueB}}", "{{@.valueA}} / {{@.valueB}}" };
        var results = new Dictionary<string, long>();
        foreach (var expr in expressions)
        {
            var calc = new Calculate(expr);
            var stopwatch = Stopwatch.StartNew();
            foreach (var item in largeDataset["calculations"]) calc.Execute(item, largeDataset, executeOptions);
            stopwatch.Stop();
            results[expr.Split(' ')[1]] = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"{expr.Split(' ')[1]} - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        }
        Assert.IsTrue(results.All(r => r.Value < 2000), "Simple arithmetic should complete under 2000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_ComplexExpressions_10000_Records()
    {
        var expressions = new[] { "{{@.valueA}} + {{@.valueB}} * {{@.valueC}} - {{@.percentage}} / 100", "({{@.valueA}} + {{@.valueB}}) / ({{@.valueC}} + 1) * {{@.multiplier}}", "{{@.valueA}} * 0.1 + {{@.valueB}} * 0.2 + {{@.valueC}} * 0.3", "(({{@.valueA}} - {{@.valueB}}) / {{@.valueC}}) + ({{@.percentage}} * {{@.multiplier}})" };
        var results = new Dictionary<string, long>();
        foreach (var expr in expressions)
        {
            var calc = new Calculate(expr);
            var stopwatch = Stopwatch.StartNew();
            foreach (var item in largeDataset["calculations"]) calc.Execute(item, largeDataset, executeOptions);
            stopwatch.Stop();
            results[expr.Substring(0, 20)] = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Complex expr - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        }
        Assert.IsTrue(results.All(r => r.Value < 5000), "Complex expressions should complete under 5000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_TokenReplacement_Stress()
    {
        var multiTokenExpr = "{{@.valueA}} + {{@.valueB}} + {{@.valueC}} + {{@.percentage}} + {{@.multiplier}} + {{@.id}}";
        var calc = new Calculate(multiTokenExpr);
        var stopwatch = Stopwatch.StartNew();
        foreach (var item in largeDataset["calculations"]) calc.Execute(item, largeDataset, executeOptions);
        stopwatch.Stop();
        Console.WriteLine($"Multi-token replacement - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Assert.Less(stopwatch.ElapsedMilliseconds, 8000, "Multi-token replacement should complete under 8000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_EuropeanDecimal_10000_Records()
    {
        var europeanData = GenerateEuropeanDataset(10000);
        var calc = new Calculate("{{@.valueA}} + {{@.valueB}}");
        var stopwatch = Stopwatch.StartNew();
        foreach (var item in europeanData["calculations"]) calc.Execute(item, europeanData, executeOptions);
        stopwatch.Stop();
        Console.WriteLine($"European decimals - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Assert.Less(stopwatch.ElapsedMilliseconds, 3000, "European decimal processing should complete under 3000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_DivisionByZero_Detection()
    {
        var zeroData = GenerateZeroDataset(1000);
        var calc = new Calculate("{{@.valueA}} / {{@.valueB}}");
        var stopwatch = Stopwatch.StartNew();
        foreach (var item in zeroData["calculations"]) calc.Execute(item, zeroData, executeOptions);
        stopwatch.Stop();
        Console.WriteLine($"Division by zero detection - 1,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "Division by zero detection should complete under 1000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceComparison_ExpressionComplexity()
    {
        var expressions = new Dictionary<string, string> { ["Simple"] = "{{@.valueA}} + {{@.valueB}}", ["Medium"] = "{{@.valueA}} + {{@.valueB}} * {{@.valueC}}", ["Complex"] = "({{@.valueA}} + {{@.valueB}}) / ({{@.valueC}} + 1) * {{@.multiplier}}", ["Very Complex"] = "(({{@.valueA}} - {{@.valueB}}) / {{@.valueC}}) + ({{@.percentage}} * {{@.multiplier}} / 100)" };
        var results = new Dictionary<string, long>();
        var testData = GenerateDataset(1000);
        foreach (var expr in expressions)
        {
            var calc = new Calculate(expr.Value);
            var sw = Stopwatch.StartNew();
            foreach (var item in testData["calculations"]) calc.Execute(item, testData, executeOptions);
            sw.Stop();
            results[expr.Key] = sw.ElapsedMilliseconds;
        }
        Console.WriteLine("\n=== Expression Complexity Comparison (1,000 records) ===");
        foreach (var kvp in results.OrderBy(x => x.Value)) Console.WriteLine($"{kvp.Key}: {kvp.Value}ms ({(double)kvp.Value / 1000:F3}ms per record)");
    }

    [Test]
    [Category("Performance")]
    public void MemoryUsageTest()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(false);
        var calc = new Calculate("{{@.valueA}} + {{@.valueB}} * {{@.valueC}}");
        foreach (var item in largeDataset["calculations"]) calc.Execute(item, largeDataset, executeOptions);
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;
        Console.WriteLine($"Memory used: {memoryUsed / 1024 / 1024:F2} MB");
        Console.WriteLine($"Memory per record: {memoryUsed / 10000:F0} bytes");
        Assert.Less(memoryUsed, 50 * 1024 * 1024, $"Used {memoryUsed / 1024 / 1024:F2} MB, expected under 50 MB");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_SingleCalculation_Microseconds()
    {
        var singleItem = largeDataset["calculations"][0];
        var calc = new Calculate("{{@.valueA}} + {{@.valueB}} * {{@.valueC}}");
        calc.Execute(singleItem, largeDataset, executeOptions);
        var stopwatch = Stopwatch.StartNew();
        var result = calc.Execute(singleItem, largeDataset, executeOptions);
        stopwatch.Stop();
        Assert.IsTrue(result.Success);
        LogPerformanceResults("Single calculation", stopwatch, 1);
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_RegexPattern_Optimization()
    {
        var noTokenExpr = "123.45 + 678.90";
        var oneTokenExpr = "{{@.valueA}} + 100";
        var multiTokenExpr = "{{@.valueA}} + {{@.valueB}} + {{@.valueC}}";
        var results = new Dictionary<string, long>();
        var testData = GenerateDataset(5000);
        var expressions = new Dictionary<string, string> { ["No Tokens"] = noTokenExpr, ["One Token"] = oneTokenExpr, ["Multi Tokens"] = multiTokenExpr };
        foreach (var expr in expressions)
        {
            var calc = new Calculate(expr.Value);
            var sw = Stopwatch.StartNew();
            foreach (var item in testData["calculations"]) calc.Execute(item, testData, executeOptions);
            sw.Stop();
            results[expr.Key] = sw.ElapsedMilliseconds;
        }
        Console.WriteLine("\n=== Token Processing Comparison (5,000 records) ===");
        foreach (var kvp in results) Console.WriteLine($"{kvp.Key}: {kvp.Value}ms ({(double)kvp.Value / 5000:F3}ms per record)");
    }

    private JToken GenerateEuropeanDataset(int size)
    {
        var calculations = new JArray();
        var random = new Random(42);
        for (int i = 0; i < size; i++)
        {
            calculations.Add(new JObject
            {
                ["valueA"] = $"{Math.Round(random.NextDouble() * 1000, 2):F2}".Replace('.', ','),
                ["valueB"] = $"{Math.Round(random.NextDouble() * 100 + 1, 2):F2}".Replace('.', ',')
            });
        }
        return new JObject { ["calculations"] = calculations };
    }

    private JToken GenerateZeroDataset(int size)
    {
        var calculations = new JArray();
        var random = new Random(42);
        for (int i = 0; i < size; i++)
        {
            calculations.Add(new JObject
            {
                ["valueA"] = Math.Round(random.NextDouble() * 1000, 2),
                ["valueB"] = random.NextDouble() < 0.1 ? 0 : Math.Round(random.NextDouble() * 100 + 1, 2)
            });
        }
        return new JObject { ["calculations"] = calculations };
    }

    private void LogPerformanceResults(string testName, Stopwatch stopwatch, int recordCount = 1)
    {
        var milliseconds = stopwatch.ElapsedMilliseconds;
        var microseconds = GetMicroseconds(stopwatch);
        Console.WriteLine($"{testName}:");
        Console.WriteLine($"  Total: {milliseconds}ms ({microseconds}us)");
        if (recordCount > 1) Console.WriteLine($"  Per record: {(double)milliseconds / recordCount:F3}ms ({microseconds / recordCount:F0}us)");
    }

    private long GetMicroseconds(Stopwatch stopwatch)
    {
        return stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
    }

    [TearDown]
    public void TearDown()
    {
        function = new Calculate();
    }
}