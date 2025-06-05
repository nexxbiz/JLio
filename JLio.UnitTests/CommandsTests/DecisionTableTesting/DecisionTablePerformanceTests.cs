using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JLio.Commands;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests.DecisionTableTesting;

[TestFixture]
public class DecisionTablePerformanceTests
{
    private IExecutionContext executeOptions;
    private JToken largeDataset;
    private DecisionTable command;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        GenerateLargeDataset();
        SetupDecisionTableCommand();
    }

    private void GenerateLargeDataset()
    {
        var customers = new JArray();
        var random = new Random(42); // Fixed seed for reproducible results

        // Generate 10,000 customers
        for (int i = 0; i < 10000; i++)
        {
            var customer = new JObject
            {
                ["id"] = i,
                ["age"] = random.Next(18, 80),
                ["membershipLevel"] = GetRandomMembership(random),
                ["orderValue"] = Math.Round((decimal)(random.NextDouble() * 1000), 2),
                ["country"] = GetRandomCountry(random),
                ["yearsActive"] = random.Next(0, 20),
                ["creditScore"] = random.Next(300, 850),
                ["lastOrderDays"] = random.Next(0, 365),
                ["category"] = GetRandomCategory(random)
            };
            customers.Add(customer);
        }

        largeDataset = new JObject { ["customers"] = customers };
    }

    private string GetRandomMembership(Random random)
    {
        var levels = new[] { "bronze", "silver", "gold", "platinum" };
        return levels[random.Next(levels.Length)];
    }

    private string GetRandomCountry(Random random)
    {
        var countries = new[] { "US", "CA", "UK", "DE", "FR", "AU", "JP" };
        return countries[random.Next(countries.Length)];
    }

    private string GetRandomCategory(Random random)
    {
        var categories = new[] { "electronics", "clothing", "books", "home", "sports" };
        return categories[random.Next(categories.Length)];
    }

    private void SetupDecisionTableCommand()
    {
        command = new DecisionTable
        {
            Path = "$.customers[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "membershipLevel", Path = "@.membershipLevel", Type = "string" },
                    new DecisionInput { Name = "orderValue", Path = "@.orderValue", Type = "number" },
                    new DecisionInput { Name = "country", Path = "@.country", Type = "string" },
                    new DecisionInput { Name = "creditScore", Path = "@.creditScore", Type = "number" },
                    new DecisionInput { Name = "yearsActive", Path = "@.yearsActive", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "discountRate", Path = "@.pricing.discount" },
                    new DecisionOutput { Name = "riskLevel", Path = "@.risk.level" },
                    new DecisionOutput { Name = "tier", Path = "@.customerTier" },
                    new DecisionOutput { Name = "offers", Path = "@.marketing.offers" }
                },
                Rules = new List<DecisionRule>
                {
                    // Complex rule with multiple conditions
                    new DecisionRule
                    {
                        Priority = 100,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=65" },
                            { "membershipLevel", new JArray("gold", "platinum") },
                            { "orderValue", ">=500" },
                            { "creditScore", ">=750" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.25 },
                            { "riskLevel", "low" },
                            { "tier", "premium_senior" },
                            { "offers", new JArray("senior_discount", "premium_shipping") }
                        }
                    },
                    // Complex condition with AND/OR
                    new DecisionRule
                    {
                        Priority = 90,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=25 && <=45" },
                            { "membershipLevel", "platinum" },
                            { "orderValue", ">=200" },
                            { "country", new JArray("US", "CA", "UK") }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.20 },
                            { "riskLevel", "low" },
                            { "tier", "platinum_prime" },
                            { "offers", new JArray("fast_shipping", "loyalty_bonus") }
                        }
                    },
                    // Range conditions
                    new DecisionRule
                    {
                        Priority = 80,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "creditScore", ">=700 && <=800" },
                            { "yearsActive", ">=2" },
                            { "orderValue", ">100" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.15 },
                            { "riskLevel", "medium" },
                            { "tier", "trusted" },
                            { "offers", new JArray("credit_offer") }
                        }
                    },
                    // OR conditions
                    new DecisionRule
                    {
                        Priority = 70,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "membershipLevel", "gold" },
                            { "orderValue", ">=300 || <=50 && >=500" }, // Complex OR with AND
                            { "yearsActive", ">=1" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.10 },
                            { "riskLevel", "medium" },
                            { "tier", "gold_member" },
                            { "offers", new JArray("gold_benefits") }
                        }
                    },
                    // High volume rule
                    new DecisionRule
                    {
                        Priority = 60,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "orderValue", ">=1000" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.30 },
                            { "riskLevel", "low" },
                            { "tier", "high_value" },
                            { "offers", new JArray("vip_treatment") }
                        }
                    },
                    // Catch-all rule
                    new DecisionRule
                    {
                        Priority = 10,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=18" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discountRate", 0.05 },
                            { "riskLevel", "standard" },
                            { "tier", "standard" },
                            { "offers", new JArray("welcome_offer") }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "discountRate", 0.0 },
                    { "riskLevel", "unknown" },
                    { "tier", "unclassified" },
                    { "offers", new JArray() }
                }
            }
        };
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_FirstMatch_10000_Records()
    {
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "firstMatch",
            ConflictResolution = "priority",
            StopOnError = false
        };

        var stopwatch = Stopwatch.StartNew();
        var result = command.Execute(largeDataset, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Console.WriteLine($"FirstMatch - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / 10000:F3}ms");

        // Performance assertion - should be under 5 seconds for 10k records
        Assert.Less(stopwatch.ElapsedMilliseconds, 5000,
            $"FirstMatch took {stopwatch.ElapsedMilliseconds}ms, expected under 5000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_AllMatches_10000_Records()
    {
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "allMatches",
            ConflictResolution = "merge",
            StopOnError = false
        };

        var stopwatch = Stopwatch.StartNew();
        var result = command.Execute(largeDataset, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Console.WriteLine($"AllMatches - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / 10000:F3}ms");

        // AllMatches should be slower but reasonable
        Assert.Less(stopwatch.ElapsedMilliseconds, 15000,
            $"AllMatches took {stopwatch.ElapsedMilliseconds}ms, expected under 15000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_BestMatch_10000_Records()
    {
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "bestMatch",
            ConflictResolution = "priority",
            StopOnError = false
        };

        var stopwatch = Stopwatch.StartNew();
        var result = command.Execute(largeDataset, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Console.WriteLine($"BestMatch - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / 10000:F3}ms");

        // BestMatch evaluates all rules so should be similar to AllMatches
        Assert.Less(stopwatch.ElapsedMilliseconds, 15000,
            $"BestMatch took {stopwatch.ElapsedMilliseconds}ms, expected under 15000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_ComplexConditions_Stress()
    {
        // Create a command with very complex conditions to stress test the condition evaluator
        var complexCommand = new DecisionTable
        {
            Path = "$.customers[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "orderValue", Path = "@.orderValue", Type = "number" },
                    new DecisionInput { Name = "creditScore", Path = "@.creditScore", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "result", Path = "@.complexResult" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            // Super complex condition
                            { "age", ">=18 && <=25 || >=65 && <=80" },
                            { "orderValue", ">=100 && <=500 || >=1000 && <=2000" },
                            { "creditScore", ">=300 && <=600 || >=750 && <=850" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "result", "complex_match" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "result", "no_match" }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "firstMatch",
                    ConflictResolution = "priority",
                    StopOnError = false
                }
            }
        };

        var stopwatch = Stopwatch.StartNew();
        var result = complexCommand.Execute(largeDataset, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Console.WriteLine($"Complex Conditions - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / 10000:F3}ms");

        // Complex condition parsing should still be reasonable
        Assert.Less(stopwatch.ElapsedMilliseconds, 8000,
            $"Complex conditions took {stopwatch.ElapsedMilliseconds}ms, expected under 8000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_ManyRules_Scaling()
    {
        // Test with many rules to see how rule count affects performance
        var manyRulesCommand = CreateCommandWithManyRules(50); // 50 rules

        var stopwatch = Stopwatch.StartNew();
        var result = manyRulesCommand.Execute(largeDataset, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Console.WriteLine($"Many Rules (50) - 10,000 records: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / 10000:F3}ms");

        // Many rules should scale linearly
        Assert.Less(stopwatch.ElapsedMilliseconds, 20000,
            $"Many rules took {stopwatch.ElapsedMilliseconds}ms, expected under 20000ms");
    }

    [Test]
    [Category("Performance")]
    public void PerformanceComparison_AllModes()
    {
        var results = new Dictionary<string, long>();

        // Test FirstMatch
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "firstMatch",
            ConflictResolution = "priority"
        };
        var sw = Stopwatch.StartNew();
        command.Execute(largeDataset, executeOptions);
        sw.Stop();
        results["FirstMatch"] = sw.ElapsedMilliseconds;

        // Test AllMatches
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "allMatches",
            ConflictResolution = "merge"
        };
        sw.Restart();
        command.Execute(largeDataset, executeOptions);
        sw.Stop();
        results["AllMatches"] = sw.ElapsedMilliseconds;

        // Test BestMatch
        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "bestMatch",
            ConflictResolution = "priority"
        };
        sw.Restart();
        command.Execute(largeDataset, executeOptions);
        sw.Stop();
        results["BestMatch"] = sw.ElapsedMilliseconds;

        // Print comparison
        Console.WriteLine("\n=== Performance Comparison (10,000 records) ===");
        foreach (var kvp in results.OrderBy(x => x.Value))
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}ms ({(double)kvp.Value / 10000:F3}ms per record)");
        }
    }

    [Test]
    [Category("Performance")]
    public void MemoryUsageTest()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryBefore = GC.GetTotalMemory(false);

        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "allMatches",
            ConflictResolution = "merge"
        };

        var result = command.Execute(largeDataset, executeOptions);

        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        Assert.IsTrue(result.Success);
        Console.WriteLine($"Memory used: {memoryUsed / 1024 / 1024:F2} MB");
        Console.WriteLine($"Memory per record: {memoryUsed / 10000:F0} bytes");

        // Should not use excessive memory (arbitrary limit)
        Assert.Less(memoryUsed, 100 * 1024 * 1024,
            $"Used {memoryUsed / 1024 / 1024:F2} MB, expected under 100 MB");
    }

    private DecisionTable CreateCommandWithManyRules(int ruleCount)
    {
        var rules = new List<DecisionRule>();
        var random = new Random(42);

        for (int i = 0; i < ruleCount; i++)
        {
            rules.Add(new DecisionRule
            {
                Priority = random.Next(1, 100),
                Conditions = new Dictionary<string, JToken>
                {
                    { "age", $">={random.Next(18, 70)}" },
                    { "orderValue", $">={random.Next(50, 1000)}" }
                },
                Results = new Dictionary<string, JToken>
                {
                    { "tier", $"tier_{i}" }
                }
            });
        }

        return new DecisionTable
        {
            Path = "$.customers[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "orderValue", Path = "@.orderValue", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "tier", Path = "@.tier" }
                },
                Rules = rules,
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "tier", "default" }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "firstMatch",
                    ConflictResolution = "priority"
                }
            }
        };
    }

    private void LogPerformanceResults(string testName, Stopwatch stopwatch, int recordCount = 1)
    {
        var milliseconds = stopwatch.ElapsedMilliseconds;
        var microseconds = GetMicroseconds(stopwatch);

        Console.WriteLine($"{testName}:");
        Console.WriteLine($"  Total: {milliseconds}ms ({microseconds}us)");

        if (recordCount > 1)
        {
            Console.WriteLine($"  Per record: {(double)milliseconds / recordCount:F3}ms ({microseconds / recordCount:F0}us)");
        }
    }

    private long GetMicroseconds(Stopwatch stopwatch)
    {
        return stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
    }

    [Test]
    [Category("Performance")]
    public void PerformanceTest_SingleRecord_Microseconds()
    {
        // Test with just one record for precise microsecond measurements
        var singleCustomer = new JObject
        {
            ["customers"] = new JArray { largeDataset["customers"][0] }
        };

        command.DecisionTableConfig.ExecutionStrategy = new ExecutionStrategy
        {
            Mode = "firstMatch",
            ConflictResolution = "priority",
            StopOnError = false
        };

        // Warm up
        command.Execute(singleCustomer, executeOptions);

        // Actual measurement
        var stopwatch = Stopwatch.StartNew();
        var result = command.Execute(singleCustomer, executeOptions);
        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        LogPerformanceResults("Single record processing", stopwatch, 1);
    }
    public void TearDown()
    {
        // Reset data for next test
        SetupDecisionTableCommand();
    }
}