using System.Collections.Generic;
using System.Linq;
using JLio.Commands;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests.DecisionTableTesting;

public class DecisionTableTests
{
    private JToken data;
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        data = JToken.Parse(@"{
                ""customers"": [
                    {
                        ""id"": 1,
                        ""age"": 25,
                        ""membershipLevel"": ""gold"",
                        ""country"": ""US"",
                        ""orderValue"": 150
                    },
                    {
                        ""id"": 2,
                        ""age"": 70,
                        ""membershipLevel"": ""silver"",
                        ""country"": ""CA"",
                        ""orderValue"": 500
                    },
                    {
                        ""id"": 3,
                        ""age"": 30,
                        ""membershipLevel"": ""bronze"",
                        ""country"": ""UK"",
                        ""orderValue"": 75
                    }
                ],
                ""singleCustomer"": {
                    ""age"": 22,
                    ""membershipLevel"": ""gold"",
                    ""country"": ""US""
                }
            }");
    }

    [Test]
    public void CanExecuteSimpleDecisionTable()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "membership", Path = "@.membershipLevel", Type = "string" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "discount", Path = "@.discount" },
                    new DecisionOutput { Name = "category", Path = "@.category" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=18" },
                            { "membership", "gold" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discount", 0.3 },
                            { "category", "premium" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "discount", 0.0 },
                    { "category", "standard" }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var customer = data.SelectToken("$.singleCustomer");
        Assert.AreEqual(0.3, customer.SelectToken("$.discount").ToObject<decimal>());
        Assert.AreEqual("premium", customer.SelectToken("$.category").ToString());
    }

    [Test]
    public void CanExecuteDecisionTableOnArray()
    {
        var command = new DecisionTable
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
                    new DecisionOutput { Name = "discount", Path = "@.pricing.discount" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Priority = 1,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=65" },
                            { "orderValue", ">=100" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discount", 0.25 }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 2,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "orderValue", ">=100" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "discount", 0.1 }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "discount", 0.0 }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Customer 1: age=25, orderValue=150 -> should get 0.1 discount (second rule)
        var customer1 = data.SelectToken("$.customers[0]");
        Assert.AreEqual(0.1, customer1.SelectToken("$.pricing.discount").ToObject<decimal>());

        // Customer 2: age=70, orderValue=500 -> should get 0.25 discount (first rule)
        var customer2 = data.SelectToken("$.customers[1]");
        Assert.AreEqual(0.25, customer2.SelectToken("$.pricing.discount").ToObject<decimal>());

        // Customer 3: age=30, orderValue=75 -> should get 0.0 discount (default)
        var customer3 = data.SelectToken("$.customers[2]");
        Assert.AreEqual(0.0, customer3.SelectToken("$.pricing.discount").ToObject<decimal>());
    }

    [Test]
    public void CanHandleArrayMembershipConditions()
    {
        var command = new DecisionTable
        {
            Path = "$.customers[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "country", Path = "@.country", Type = "string" },
                    new DecisionInput { Name = "membership", Path = "@.membershipLevel", Type = "string" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "shippingCost", Path = "@.shipping.cost" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "country", new JArray("US", "CA") },
                            { "membership", new JArray("gold", "silver") }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "shippingCost", 5.0 }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "shippingCost", 15.0 }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Customer 1: US + gold -> 5.0
        var customer1 = data.SelectToken("$.customers[0]");
        Assert.AreEqual(5.0, customer1.SelectToken("$.shipping.cost").ToObject<decimal>());

        // Customer 2: CA + silver -> 5.0
        var customer2 = data.SelectToken("$.customers[1]");
        Assert.AreEqual(5.0, customer2.SelectToken("$.shipping.cost").ToObject<decimal>());

        // Customer 3: UK + bronze -> 15.0 (default)
        var customer3 = data.SelectToken("$.customers[2]");
        Assert.AreEqual(15.0, customer3.SelectToken("$.shipping.cost").ToObject<decimal>());
    }

    [Test]
    public void CanHandleAbsolutePaths()
    {
        var command = new DecisionTable
        {
            Path = "$.customers[0]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "customerAge", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "otherCustomerAge", Path = "$.customers[1].age", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "comparison", Path = "@.ageComparison" },
                    new DecisionOutput { Name = "globalFlag", Path = "$.globalSettings.ageFlag" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "customerAge", "<50" },
                            { "otherCustomerAge", ">=50" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "comparison", "younger" },
                            { "globalFlag", "mixed_ages" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "comparison", "same_range" },
                    { "globalFlag", "uniform_ages" }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var customer1 = data.SelectToken("$.customers[0]");
        Assert.AreEqual("younger", customer1.SelectToken("$.ageComparison").ToString());
        Assert.AreEqual("mixed_ages", data.SelectToken("$.globalSettings.ageFlag").ToString());
    }

    [Test]
    public void CanValidateRequiredProperties()
    {
        var command = new DecisionTable
        {
            Path = "", // Missing path
            DecisionTableConfig = null // Missing config
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);

        var logEntries = executeOptions.GetLogEntries();
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("Path property for decisionTable command is missing")));
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("DecisionTable property for decisionTable command is missing")));
    }

    [Test]
    public void CanValidateConfigurationProperties()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>(), // Empty inputs
                Outputs = null, // Missing outputs
                Rules = null // Missing rules
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);

        var logEntries = executeOptions.GetLogEntries();
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("DecisionTable inputs are required")));
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("DecisionTable outputs are required")));
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("DecisionTable rules are required")));
    }

    [Test]
    public void CanHandleNoMatchingRulesWithDefaults()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "category", Path = "@.category" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=65" } // Won't match age=22
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "category", "senior" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "category", "standard" }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var customer = data.SelectToken("$.singleCustomer");
        Assert.AreEqual("standard", customer.SelectToken("$.category").ToString());
    }

    [Test]
    public void CanHandleNoMatchingRulesWithoutDefaults()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "category", Path = "@.category" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=65" } // Won't match age=22
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "category", "senior" }
                        }
                    }
                }
                // No default results
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var customer = data.SelectToken("$.singleCustomer");
        Assert.IsNull(customer.SelectToken("$.category")); // Should not be set
    }

    [Test]
    public void CanHandleComparisonOperators()
    {
        var command = new DecisionTable
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
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">65" },
                            { "orderValue", "<=200" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "tier", "senior_budget" }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", "<30" },
                            { "orderValue", ">100" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "tier", "young_spender" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "tier", "standard" }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Customer 1: age=25, orderValue=150 -> young_spender
        var customer1 = data.SelectToken("$.customers[0]");
        Assert.AreEqual("young_spender", customer1.SelectToken("$.tier").ToString());

        // Customer 2: age=70, orderValue=500 -> standard (doesn't match senior_budget because orderValue > 200)
        var customer2 = data.SelectToken("$.customers[1]");
        Assert.AreEqual("standard", customer2.SelectToken("$.tier").ToString());

        // Customer 3: age=30, orderValue=75 -> standard
        var customer3 = data.SelectToken("$.customers[2]");
        Assert.AreEqual("standard", customer3.SelectToken("$.tier").ToString());
    }

    [Test]
    public void CanHandleMissingInputPaths()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" },
                    new DecisionInput { Name = "nonExistent", Path = "@.nonExistentField", Type = "string" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "result", Path = "@.result" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=18" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "result", "adult" }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, JToken>
                {
                    { "result", "unknown" }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var customer = data.SelectToken("$.singleCustomer");
        Assert.AreEqual("adult", customer.SelectToken("$.result").ToString());

        // Should have warning about missing input
        var logEntries = executeOptions.GetLogEntries();
        Assert.IsTrue(logEntries.Any(l => l.Message.Contains("Input 'nonExistent' evaluated to: null")));
    }

    [Test]
    public void CanUseEmptyConstructor()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "category", Path = "@.category" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=18" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "category", "adult" }
                        }
                    }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("adult", data.SelectToken("$.singleCustomer.category").ToString());
    }

    [Test]
    public void OutputsCanUseFunctions()
    {
        var command = new DecisionTable
        {
            Path = "$.singleCustomer",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "uniqueId", Path = "@.uid" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "age", ">=18" }
                        },
                        Results = new Dictionary<string, JToken>
                        {
                            { "uniqueId", "=newGuid()" }
                        }
                    }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        var uid = data.SelectToken("$.singleCustomer.uid").ToString();
        Assert.IsNotEmpty(uid);
    }
}